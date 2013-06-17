using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;



namespace TheAirline.Model.AirlineModel
{
    [Serializable] 
    //the profile for an airline
    public class AirlineProfile
    {
        
        public string Name { get; set; }
        
        public string CEO { get; set; }
        
        public string IATACode { get; set; }
        
        public Country Country { get; set; }
        
        public List<Country> Countries { get; set; }
        
        public string Color { get; set; }
        public string Logo { get { return getCurrentLogo();} private set{;} }
        
        public List<AirlineLogo> Logos { get; set; }
        
        public Airport PreferedAirport { get; set; }
        
        public int Founded { get; set; }
        
        public int Folded { get; set; }
        
        public Boolean IsReal { get; set; }
        public string Narrative { get; set; }
        public AirlineProfile(string name, string iata, string color,  string ceo, Boolean isReal, int founded, int folded)
        {
            this.Name = name;
            this.IATACode = iata;
            this.CEO = ceo;
            this.Color = color;
            this.IsReal = isReal;
            this.Founded = founded;
            this.Folded = folded;
            this.Countries = new List<Country>();
            this.Logos = new List<AirlineLogo>();
        }
        //adds a logo to the airline
        public void addLogo(AirlineLogo logo)
        {
            this.Logos.Add(logo);
        }
        //returns the current logo for the airline
        private string getCurrentLogo()
        {
            return this.Logos.Find(l => l.FromYear <= GameObject.GetInstance().GameTime.Year && l.ToYear >= GameObject.GetInstance().GameTime.Year).Path;
        }
    }
    //the class for an airline logo
    [Serializable]
    public class AirlineLogo
    {
        
        public int FromYear { get; set; }
        
        public int ToYear { get; set; }
        
        public string Path { get; set; }
        public AirlineLogo(int fromYear, int toYear, string path)
        {
            this.FromYear = fromYear;
            this.ToYear = toYear;
            this.Path = path;
        }
        public AirlineLogo(string path) : this(1900,2199,path)
        {

        }
    }
}
