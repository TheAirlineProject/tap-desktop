using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using ProtoBuf;


namespace TheAirline.Model.AirlineModel
{
    [ProtoContract] 
    //the profile for an airline
    public class AirlineProfile
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string CEO { get; set; }
        [ProtoMember(3)]
        public string IATACode { get; set; }
        [ProtoMember(4)]
        public Country Country { get; set; }
        [ProtoMember(5)]
        public List<Country> Countries { get; set; }
        [ProtoMember(6)]
        public string Color { get; set; }
        public string Logo { get { return getCurrentLogo();} private set{;} }
        [ProtoMember(7)]
        public List<AirlineLogo> Logos { get; set; }
        [ProtoMember(8)]
        public Airport PreferedAirport { get; set; }
        [ProtoMember(9)]
        public int Founded { get; set; }
        [ProtoMember(10)]
        public int Folded { get; set; }
        [ProtoMember(11)]
        public Boolean IsReal { get; set; }
        [ProtoMember(12)]
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
    [ProtoContract]
    public class AirlineLogo
    {
        [ProtoMember(1)]
        public int FromYear { get; set; }
        [ProtoMember(2)]
        public int ToYear { get; set; }
        [ProtoMember(3)]
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
