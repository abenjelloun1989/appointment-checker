using System;
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
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            if(args.Count() > 0 && args[0] == "candilib")
            {
                await ProcessCandilib();
            }
            else if(args.Count() > 0 && args[0] == "wedding")
            {
                await ProcessWeddingAppointment();
            }
        }
        private static async Task ProcessCandilib()
        {
            client.DefaultRequestHeaders.Add("X-CLIENT-ID", "61ca43b2-3ffc-4e25-8ac3-ab8fd10ef21b.2.11.2-beta1.");
            client.DefaultRequestHeaders.Add("X-REQUEST-ID", "582867fc-5602-431a-8000-cf733f7b5a3f");
            client.DefaultRequestHeaders.Add("X-USER-ID", "5e52a4e443d687001177a04e"); // const
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", 
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjVlNTJhNGU0NDNkNjg3MDAxMTc3YTA0ZSIsImxldmVsIjowLCJjYW5kaWRhdFN0YXR1cyI6IjAiLCJub21OYWlzc2FuY2UiOiJCRU5KRUxMT1VOIiwiY29kZU5lcGgiOiIxODA3OTIzMDExODUiLCJkZXBhcnRlbWVudCI6IjkxIiwiZW1haWwiOiJhYmVuamVsbG91bjE5ODlAZ21haWwuY29tIiwicG9ydGFibGUiOiIwNjAxMDY2Nzc1IiwicHJlbm9tIjoiQWJkZWxheml6IiwiZmlyc3RDb25uZWN0aW9uIjp0cnVlLCJkYXRlRVRHIjoiMjAyMy0xMS0wMyIsImlzSW5SZWNlbnRseURlcHQiOmZhbHNlLCJpYXQiOjE2MjA0NjMzMDQsImV4cCI6MTYyMDUxODM5OH0.TohfAsw8c3tJWrhi88kyGDjS9zTYoFwEDF2-rmgnpDw");

            var deps = new string[] {"78", "92", "95", "77", "93", "38", "91", "94", "69", "76", "45"};
            foreach(var dep in deps){
                var res = await HttpClientJsonExtensions.GetFromJsonAsync<List<City>>(client, 
                $"https://beta.interieur.gouv.fr/candilib/api/v2/candidat/centres?departement={dep}");
                
                if(res.Count > 0 && res.Any(r => r.count > 0))
                {
                    Console.WriteLine($"{dep} => YES !");
                    Console.Beep(700, 2400);
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine($"{dep} => bouuuuuh");
                }
            }
        }
        private static async Task ProcessWeddingAppointment()
        {
            var uri = "https://www.espace-citoyens.net/asnieres-sur-seine/espace-citoyens/RendezVous/ProcessListeCreneauxDisponibles";
            var dt = DateTime.Now;
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("type", "listeCreneaux"), 
                new KeyValuePair<string, string>("key", "39201108e884dafea5e80b104b728459"),
                new KeyValuePair<string, string>("tStamp", $"{dt:yyyyMMddhhmmss}"),
                new KeyValuePair<string, string>("idQdt", "645"),
                new KeyValuePair<string, string>("origine", "EspCitoyen"),
                new KeyValuePair<string, string>("idSit", "1"),
                new KeyValuePair<string, string>("dateDeb", $"{dt:dd/MM/yy}"),
                new KeyValuePair<string, string>("dateFin", "31/12/21"),
                new KeyValuePair<string, string>("nbCreneauxAJoindre", "1"),
                new KeyValuePair<string, string>("listeIdsReserves", "undefined")
            });

            var response = await client.PostAsync(uri, formContent);
            var stringContent = await response.Content.ReadAsStringAsync();
            if(stringContent != "[]")
            {
                Console.Beep(700, 2400); // send notification
                Console.WriteLine("YES !");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("bouuuuuh"); 
            }
        }
    }
}
