using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    //the contract for an airline for a manufacturer
    public class ManufacturerContract
    {
        public Manufacturer Manufacturer { get; set; }
        public DateTime SigningDate { get; set; }
        public int Length { get; set; }
        public DateTime ExpireDate { get; set; }
        public double Discount { get; set; } //in percent
        public int Airliners { get; set; } //the number of airliners to purchase in that period
        public int PurchasedAirliners { get; set; }
        public ManufacturerContract(Manufacturer manufacturer,  DateTime date, int length, double discount)
        {
            this.Manufacturer = manufacturer;
            this.Airliners = length;
            this.SigningDate = date;
            this.Length = length;
            this.Discount = discount;
            this.ExpireDate = date.AddYears(this.Length);
            this.PurchasedAirliners = 0;
        }
        //returns the termination fee for the contract
        public double getTerminationFee()
        {
            return GeneralHelpers.GetInflationPrice(this.Length * 1000000);
        }
    }
}
