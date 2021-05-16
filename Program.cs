using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Threading.Tasks;
using appointment_checker.models;
using System.Text.RegularExpressions;

namespace appointment_checker
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private static string _context;
        private static string _searchArea;
        private static string _emailReceiver;
        public static async Task Main(string[] args)
        {
            if (args.Count() > 0)
            {
                ParseArguments(args);
                try
                {
                    switch (_context)
                    {
                        case "candilib": { await ProcessCandilib(); break; }
                        case "wedding": { await ProcessWedding(); break; }
                        case "vaccin": { await ProcessVaccin(); break; }
                    }
                }
                catch (System.Exception ex)
                {
                    SendEmailNotification(EmailStatus.Error, ex.Message);
                }
            }
        }

        private static void ParseArguments(string[] args)
        {
            _context = args[0];
            if (args.Count() == 3)
            {
                _searchArea = args[1];
                _emailReceiver = args[2];
            }
        }

        private static async Task ProcessVaccin()
        {
            var config = ConfigurationManager.AppSettings;
            var uri = config["VaccinUri"];
            var parameters = config["VaccinParameters"];
            var searchArea = _searchArea ?? config["VaccinSearchArea"];
            var searchUrl = $"{uri}/vaccination-covid-19/{searchArea}?{parameters}";
            var locationsCodes = await SearchLocationsInArea(searchUrl);

            foreach (var locationCode in locationsCodes)
            {
                var url = $"{uri}/search_results/{locationCode}.json?{parameters}";
                var res = await HttpClientJsonExtensions.GetFromJsonAsync<VaccinResponse>(_client, url);

                if (res != null)
                {
                    if (res.total > 0 && res.availabilities.Count > 0 && res.search_result != null)
                    {
                        SendEmailNotification(EmailStatus.Sucess, $"{uri}{res.search_result.url}");
                    }
                    else if (res.search_result != null)
                    {
                        Console.WriteLine($"{res.search_result.city} => none :(");
                    }
                }
                else
                {
                    Console.WriteLine($"{locationCode} => none :(");
                }
            }
        }

        private static async Task<MatchCollection> SearchLocationsInArea(string searchUrl)
        {
            var searchLocationsResult = await _client.GetAsync(searchUrl);
            var searchLocationBody = await searchLocationsResult.Content.ReadAsStringAsync();
            var template = "(?<=id=\"search-result-)\\d+";
            var regex = new Regex(@template);
            var locationsCodes = regex.Matches(searchLocationBody);
            return locationsCodes;
        }

        private static async Task ProcessCandilib()
        {
            var config = ConfigurationManager.AppSettings;
            var uri = config["CandilibUri"];
            _client.DefaultRequestHeaders.Add("X-USER-ID", config["UserId"]);
            _client.DefaultRequestHeaders.Add("X-CLIENT-ID", config["ClientId"]);
            _client.DefaultRequestHeaders.Add("X-REQUEST-ID", config["RequestId"]);
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
                config["AuthorizationBearer"]);

            foreach (var dep in new string[] { "78", "92", "95", "77", "93", "38", "91", "94", "69", "76", "45" })
            {
                var res = await HttpClientJsonExtensions.GetFromJsonAsync<List<City>>(_client,
                $"{uri}?departement={dep}");

                if (res.Count > 0 && res.Any(r => r.count > 0))
                {
                    SendEmailNotification(EmailStatus.Sucess, dep);
                }
                else
                {
                    Console.WriteLine($"{dep} => none :(");
                }
            }
        }
        
        private static async Task ProcessWedding()
        {
            var config = ConfigurationManager.AppSettings;
            var uri = config["WeddingUri"];
            var dt = DateTime.Now;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", config["Type"]),
                new KeyValuePair<string, string>("key", config["Key"]),
                new KeyValuePair<string, string>("tStamp", $"{dt:yyyyMMddhhmmss}"),
                new KeyValuePair<string, string>("idQdt", config["IdQdt"]),
                new KeyValuePair<string, string>("origine", config["Origine"]),
                new KeyValuePair<string, string>("idSit", config["IdSit"]),
                new KeyValuePair<string, string>("dateDeb", $"{dt:dd/MM/yy}"),
                new KeyValuePair<string, string>("dateFin", config["DateFin"]),
                new KeyValuePair<string, string>("nbCreneauxAJoindre", config["NbCreneauxAJoindreUri"]),
                new KeyValuePair<string, string>("listeIdsReserves", config["ListeIdsReserves"])
            });

            var response = await _client.PostAsync(uri, formContent);
            var stringContent = await response.Content.ReadAsStringAsync();
            if (stringContent != "[]")
            {
                SendEmailNotification(EmailStatus.Sucess, stringContent);
            }
            else
            {       
                Console.WriteLine($"=> none :(");
            }
        }

        private static void SendEmailNotification(EmailStatus status, string body)
        {
            var config = ConfigurationManager.AppSettings;
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(config["EmailSender"], config["EmailSenderPassword"]),
                EnableSsl = true,
            };

            var emailSender = config["EmailSender"];
            var emailReceiver = _emailReceiver ?? config["EmailReceiver"];

            var message = new MailMessage(emailSender,
                            status == EmailStatus.Sucess ? emailReceiver : emailSender);
            message.CC.Add(config["EmailReceiver"]);
            message.Subject = $"Appointment Checker {_context} : {status.ToString("g")}";
            message.Body = $"{_context} response : {body}";
                
            smtpClient.Send(message);
        }
    }
    
    public enum EmailStatus
    {
        Sucess,
        Error
    }
}
