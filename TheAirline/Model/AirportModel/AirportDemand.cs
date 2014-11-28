using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.GeneralModel.Helpers.DatabaseHelpersModel;
using TheAirline.Model.GeneralModel.Helpers.DatabaseHelpersModel.SQLite;

namespace TheAirline.Model.AirportModel
{
    //the class for the airport demand
    public class AirportDemand
    {
        public Airport Airport { get; set; }
        public AirportDemand(Airport airport)
        {
            this.Airport = airport;
        }
        public void addDemand(Airport destination, int paxvalue, int cargovalue)
        {
           // DatabaseHelpers.AddObject(demand);

            /*
            AirportDestinationDemand demand = getDemand(destination);
            
            if (demand != null)
            {
                demand.Cargo += cargovalue;
                demand.Passengers += paxvalue;

                DatabaseHelpers.UpdateObject(demand);
            }
            else*/
            DatabaseHelpers.AddObject(new AirportDestinationDemand() { Airport = this.Airport.Profile.IATACode, Destination = destination.Profile.IATACode, Cargo = cargovalue, Passengers = paxvalue });
        }
        public AirportDestinationDemand getDemand(Airport demand)
        {
            return DatabaseHelpers.GetDemand(this.Airport, demand);
        }

    }
    //the class for an airport destination demand
    public class AirportDestinationDemand
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Destination { get; set; }
        public string Airport { get; set; }
        public int Cargo { get; set; }
        public int Passengers { get; set; }
    }
}
