using System.Collections.Generic;

namespace appointment_checker.models
{
    public class VaccinResponse
    {
        public List<dynamic> availabilities { get; set; }
        public string message { get; set; }
        public SearchResult search_result { get; set; }
        public int total { get; set; }

    }
}