using System;
using System.Globalization;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.Model.AirlineModel
{
    [Serializable]
    public class AirlineInsurance
    {
        public enum InsuranceScope
        {
            Airport,
            Domestic,
            Hub,
            Global
        }

        public enum InsuranceType
        {
            PublicLiability,
            PassengerLiability,
            CombinedSingleLimit,
            FullCoverage
        }

        public enum PaymentTerms
        {
            Annual,
            Biannual,
            Quarterly,
            Monthly
        }

        public AirlineInsurance(InsuranceType insType, InsuranceScope insScope, PaymentTerms paymentTerms, int insAmount)
        {
            Deductible = 0;
            TermLength = 0;
            CancellationFee = 0;
            InsuredAmount = insAmount;
            InsType = insType;
            InsScope = insScope;
            InsTerms = paymentTerms;
        }

        public InsuranceType InsType { get; set; }

        public InsuranceScope InsScope { get; set; }

        public PaymentTerms InsTerms { get; set; }

        public int InsuredAmount { get; set; }

        public double Deductible { get; set; }

        public double TermLength { get; set; }

        public double PaymentAmount { get; set; }

        public int CancellationFee { get; set; }

        public string PolicyIndex { get; set; }

        public bool AllFleetAirliners { get; set; }

        public DateTime InsuranceEffective { get; set; }
        public DateTime InsuranceExpires { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public double RemainingPayments { get; set; }
    }

    [Serializable]
    public class InsuranceClaim
    {
        public InsuranceClaim(Airline airline, FleetAirliner airliner, Airport airport, DateTime date, int damage)
        {
            Damage = damage;
            Airline = airline;
            Airliner = airliner;
            Airport = airport;
            Date = date;
            Index = GameObject.GetInstance().GameTime.ToString(CultureInfo.InvariantCulture) + airline + damage.ToString(CultureInfo.InvariantCulture);
            SettlementDate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddMonths(2), GameObject.GetInstance().GameTime.AddMonths(24));
        }

        public Airline Airline { get; set; }
        public AirlineInsurance Policy { get; set; }

        public int Damage { get; set; }

        public DateTime Date { get; set; }
        public FleetAirliner Airliner { get; set; }
        public Airport Airport { get; set; }

        public DateTime SettlementDate { get; set; }

        public string Index { get; set; }
    }
}