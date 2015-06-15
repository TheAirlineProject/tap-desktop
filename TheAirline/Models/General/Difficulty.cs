namespace TheAirline.Models.General
{
    public class Difficulty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double AiLevel { get; set; }
        public double MoneyLevel { get; set; }
        public double LoanLevel { get; set; }
        public double PassengersLevel { get; set; }
        public double PriceLevel { get; set; }
        public double StartDataLevel { get; set; }
    }
}
