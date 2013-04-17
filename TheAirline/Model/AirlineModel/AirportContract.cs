using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirportModel;

namespace TheAirline.Model.AirlineModel
{
    //the class for a contract at an airport for an airline
    public class AirportContract
    {
        public Airline Airline { get; set; }
        public Airport Airport { get; set; }
        public DateTime ContractDate { get; set; }
        public int Length { get; set; }
        public double YearlyPayment { get; set; }
        public int NumberOfGates { get; set; }
        public AirportContract(Airline airline, Airport airport, DateTime date, int numberOfGates, int length, double yearlyPayment)
        {
            this.Airline = airline;
            this.Airport = airport;
            this.ContractDate = date;
            this.Length = length;
            this.YearlyPayment = yearlyPayment;
            this.NumberOfGates = numberOfGates;
        }
    }
}
