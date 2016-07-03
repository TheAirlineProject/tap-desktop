using System;

namespace TheAirline.Models.General.Countries
{
    public sealed class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public double Exchange { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public bool SymbolOnRight { get; set; }
    }
}
