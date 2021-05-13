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
        public static async Task Main(string[] args)
        {
            if (args.Count() > 0)
            {
                _context = args[0];
                try
                {                    
                    switch (_context)
                    {
                        case "candilib" : { await ProcessCandilib(); break; }
                        case "wedding" : { await ProcessWedding(); break; }
                        case "vaccin" : { await ProcessVaccin(); break; }
                    }      
                }
                catch (System.Exception ex)
                {
                    SendEmailNotification("ERROR", ex.Message);
                }          
            }   
        }

        private static async Task ProcessVaccin()
        {
            var config = ConfigurationManager.AppSettings;
            var uri = config["VaccinUri"];
            var parameters = config["VaccinParameters"];
            var searchUrl = config["SearchUrl"];
            var locationsCodes = await SearchLocationsCodes(searchUrl);

            foreach (var locationCode in locationsCodes)
            {
                var url = $"{uri}/search_results/{locationCode}.json?{parameters}";
                var res = await HttpClientJsonExtensions.GetFromJsonAsync<VaccinResponse>(_client, url);

                if (res != null)
                {
                    if (res.total > 0 && res.availabilities.Count > 0 && res.search_result != null)
                    {
                        SendEmailNotification("!! SUCCESS !!", $"{uri}{res.search_result.url}");
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

        private static async Task<MatchCollection> SearchLocationsCodes(string searchUrl)
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
                    SendEmailNotification("!! SUCCESS !!", dep);
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
                SendEmailNotification("!! SUCCESS !!", stringContent);
            }
            else
            {       
                Console.WriteLine($"=> none :(");
            }
        }
        private static void TriggerFound(string param = "")
        {
            Console.WriteLine($"{param} => YES !");
            Console.Beep();
            Console.ReadLine();
        }

        private static void SendEmailNotification(string status, string body)
        {
            var config = ConfigurationManager.AppSettings;
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(config["EmailSender"], config["EmailSenderPassword"]),
                EnableSsl = true,
            };
                
            smtpClient.Send(config["EmailSender"], 
                            config["EmailReceiver"],
                            $"Appointment Checker {_context} : {status}",
                            $"{_context} response : {body}");

        }
    }
}
