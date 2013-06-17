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


namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    public class AirlineInsurance
    {
        public enum InsuranceType { Public_Liability, Passenger_Liability, Combined_Single_Limit, Full_Coverage }
        public enum InsuranceScope { Airport, Domestic, Hub, Global }
        public enum PaymentTerms { Annual, Biannual, Quarterly, Monthly }
        
        public InsuranceType InsType { get; set; }
        
        public InsuranceScope InsScope { get; set; }
        
        public PaymentTerms InsTerms { get; set; }
        
        public int InsuredAmount { get; set; }
        
        public double Deductible { get; set; }
        
        public int TermLength { get; set; }
        
        public double PaymentAmount { get; set; }
        
        public int CancellationFee { get; set; }
        
        public string PolicyIndex { get; set; }
        
        public bool AllFleetAirliners { get; set; }
        
        public DateTime InsuranceEffective { get; set; }
        public DateTime InsuranceExpires { get; set; }
        public DateTime NextPaymentDue { get; set; }
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
    [Serializable]
    public class InsuranceClaim
    {
        public Airline Airline { get; set; }
        public AirlineInsurance Policy { get; set; }
        
        public int Damage { get; set; }
        
        public DateTime Date { get; set; }
        public FleetAirliner Airliner { get; set; }
        public Airport Airport { get; set; }
        
        public DateTime SettlementDate { get; set; }
        
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
