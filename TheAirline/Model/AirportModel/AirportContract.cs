
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    [Serializable]
    //the class for a contract at an airport for an airline
    public class AirportContract
    {
        public Airline Airline { get; set; }
        public Airport Airport { get; set; }
        
        public DateTime ContractDate { get; set; }
        
        public int Length { get; set; }
        
        public double YearlyPayment { get; set; }
        
        public int NumberOfGates { get; set; }
        
        public Boolean IsExclusiveDeal { get; set; }
        public int MonthsLeft { get { return getMonthsLeft();} set { ;} }
        public Terminal Terminal { get; set; }
        
        public DateTime ExpireDate { get; set; }
        
        public Boolean PayFull { get; set; }
        public AirportContract(Airline airline, Airport airport, DateTime date, int numberOfGates, int length, double yearlyPayment,Boolean payFull = false, Boolean isExclusiveDeal = false, Terminal terminal = null)
        {
            this.PayFull = payFull;
            this.Airline = airline;
            this.Airport = airport;
            this.ContractDate = date;
            this.Length = length;
            this.YearlyPayment = yearlyPayment;
            this.NumberOfGates = numberOfGates;
            this.IsExclusiveDeal = isExclusiveDeal;
            this.Terminal = terminal;
            this.ExpireDate = this.ContractDate.AddYears(this.Length);
        }
        //returns the number of months left for the contract
        public int getMonthsLeft()
        {
            return MathHelpers.GetMonthsBetween(GameObject.GetInstance().GameTime, this.ContractDate.AddYears(this.Length));
        }
    }
}
