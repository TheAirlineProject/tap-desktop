using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirportModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.InvoicesModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.PilotModel;
using TheAirline.Model.AirlineModel;
using ProtoBuf;

namespace TheAirline.Model.AirlineModel
{
    [ProtoContract]
    public class AirlineInsurance
    {
        public enum InsuranceType { Public_Liability, Passenger_Liability, Combined_Single_Limit, Full_Coverage }
        public enum InsuranceScope { Airport, Domestic, Hub, Global }
        public enum PaymentTerms { Annual, Biannual, Quarterly, Monthly }
        [ProtoMember(1)]
        public InsuranceType InsType { get; set; }
        [ProtoMember(2)]
        public InsuranceScope InsScope { get; set; }
        [ProtoMember(3)]
        public PaymentTerms InsTerms { get; set; }
        [ProtoMember(4)]
        public int InsuredAmount { get; set; }
        [ProtoMember(5)]
        public double Deductible { get; set; }
        [ProtoMember(6)]
        public int TermLength { get; set; }
        [ProtoMember(7)]
        public double PaymentAmount { get; set; }
        [ProtoMember(8)]
        public int CancellationFee { get; set; }
        [ProtoMember(9)]
        public string PolicyIndex { get; set; }
        [ProtoMember(10)]
        public bool AllFleetAirliners { get; set; }
        [ProtoMember(11)]
        public DateTime InsuranceEffective { get; set; }
        [ProtoMember(12)]
        public DateTime InsuranceExpires { get; set; }
        [ProtoMember(13)]
        public DateTime NextPaymentDue { get; set; }
        [ProtoMember(14)]
        public int RemainingPayments { get; set; }
        public AirlineInsurance(InsuranceType insType, InsuranceScope insScope, PaymentTerms paymentTerms, int insAmount)
        {
            this.Deductible = 0;
            this.TermLength = 0;
            this.CancellationFee = 0;
            this.InsuredAmount = insAmount;
            this.InsType = insType;
            this.InsScope = insScope;
            this.InsTerms = paymentTerms;
        }

       
    }
    [ProtoContract]
    public class InsuranceClaim
    {
        [ProtoMember(1,AsReference=true)]
        public Airline Airline { get; set; }
        [ProtoMember(2,AsReference=true)]
        public AirlineInsurance Policy { get; set; }
        [ProtoMember(3)]
        public int Damage { get; set; }
        [ProtoMember(4)]
        public DateTime Date { get; set; }
        [ProtoMember(5,AsReference=true)]
        public FleetAirliner Airliner { get; set; }
        [ProtoMember(6,AsReference=true)]
        public Airport Airport { get; set; }
        [ProtoMember(7)]
        public DateTime SettlementDate { get; set; }
        [ProtoMember(8)]
        public string Index { get; set; }
        public InsuranceClaim(Airline airline, FleetAirliner airliner, Airport airport, DateTime date, int damage)
        {
            this.Damage = damage;
            this.Airline = airline;
            this.Airliner = airliner;
            this.Airport = airport;
            this.Date = date;
            this.Index = GameObject.GetInstance().GameTime.ToString() + airline.ToString() + damage.ToString();
            this.SettlementDate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddMonths(2), GameObject.GetInstance().GameTime.AddMonths(24));
        }
    }
}
