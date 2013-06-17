using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirportModel
{
    [ProtoContract]
    //the class for a contract at an airport for an airline
    public class AirportContract
    {
        [ProtoMember(1,AsReference=true)]
        public Airline Airline { get; set; }
        [ProtoMember(2,AsReference=true)]
        public Airport Airport { get; set; }
        [ProtoMember(3)]
        public DateTime ContractDate { get; set; }
        [ProtoMember(4)]
        public int Length { get; set; }
        [ProtoMember(5)]
        public double YearlyPayment { get; set; }
        [ProtoMember(6)]
        public int NumberOfGates { get; set; }
        [ProtoMember(7)]
        public Boolean IsExclusiveDeal { get; set; }
        public int MonthsLeft { get { return getMonthsLeft();} set { ;} }
        [ProtoMember(8,AsReference=true)]
        public Terminal Terminal { get; set; }
        [ProtoMember(9)]
        public DateTime ExpireDate { get; set; }
        [ProtoMember(10)]
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
