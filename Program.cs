using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using appointment_checker.models;

namespace appointment_checker
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        public static async Task Main(string[] args)
        {            
            if (args.Count() > 0 && args[0] == "candilib")
            {
                await ProcessCandilib();
            }
            else if (args.Count() > 0 && args[0] == "wedding")
            {
                await ProcessWedding();
            }   
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
                    TriggerFound(dep);
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
                TriggerFound();
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
    }
}
