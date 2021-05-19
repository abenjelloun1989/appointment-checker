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
using appointment_checker.services;

namespace appointment_checker
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private static ServicesEnum _context;
        private static string _searchArea;
        private static INotifier _notifier;
        public static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    _context = Enum.Parse<ServicesEnum>(args[0]);
                    _searchArea = args.Length > 1 ? args[1] : null;
                    InitializeNotifier(args.Length > 2 ? args[2] : null);

                    switch (_context)
                    {
                        case ServicesEnum.candilib: { await ProcessCandilib(); break; }
                        case ServicesEnum.wedding: { await ProcessWedding(); break; }
                        case ServicesEnum.vaccin: { await ProcessVaccin(); break; }
                    }
                }
                catch (System.Exception ex)
                {
                    _notifier.Notify(Status.Error, string.Empty, ex.Message);
                }
            }
        }

        private static void InitializeNotifier(string emailReceiver)
        {
            var config = ConfigurationManager.AppSettings;

            _notifier = new EmailNotifier(_context,
                                        config["EmailSender"],
                                        config["EmailSenderPassword"],
                                        emailReceiver,
                                        config["DefaultEmailReceiver"]);
        }

        private static async Task ProcessVaccin()
        {
            var config = ConfigurationManager.AppSettings;
            var uri = config["VaccinUri"];
            var parameters = config["VaccinParameters"];
            var searchArea = _searchArea ?? config["VaccinSearchArea"];
            var vaccinBlackListLocations = config["VaccinBlackListLocations"]
                                            .Split(";")
                                            .Select(b => Int32.Parse(b)).ToList();
            var searchUrl = $"{uri}/vaccination-covid-19/{searchArea}?{parameters}";
            var locationsCodes = await SearchLocationsInArea(searchUrl);

            foreach (var locationCode in locationsCodes)
            {
                var url = $"{uri}/search_results/{locationCode}.json?{parameters}";
                var res = await HttpClientJsonExtensions.GetFromJsonAsync<VaccinResponse>(_client, url);

                if (res != null)
                {
                    if (ResponseIsValid(res, vaccinBlackListLocations))
                    {
                        Console.WriteLine($"{res.search_result.city} => YES !");
                        _notifier.Notify(Status.Sucess, res.search_result.city, $"{uri}{res.search_result.url}");
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

        private static bool ResponseIsValid(VaccinResponse res, List<int> vaccinBlackListLocations)
        {
            return res.total > 0 && res.availabilities.Count > 0 
                    && res.search_result != null 
                    && !vaccinBlackListLocations.Contains(res.search_result.profile_id);
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
                    _notifier.Notify(Status.Sucess, dep, dep);
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
                _notifier.Notify(Status.Sucess, "wedding", stringContent);
            }
            else
            {       
                Console.WriteLine($"=> none :(");
            }
        }
    }
}
