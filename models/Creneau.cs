namespace appointment_checker.models
{
    public class Creneau
    {
        public string id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string allDay { get; set; }
        public string description { get; set; }
        public string uid { get; set; }
        public string isDisponible { get; set; }
        public string isAccesEspacitCrn { get; set; }        
    }
}