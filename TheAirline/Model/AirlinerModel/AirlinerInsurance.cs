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

namespace TheAirline.Model.AirlinerModel
{
    [Serializable]
    public class AirlinerInsurance
    {
        public enum InsuranceType { None, Liability, Ground_Parked, Ground_Taxi,Combined_Ground, In_Flight, Full_Coverage }
        public enum InsuranceScope { Airport, Domestic, Hub, Global }
        public enum PaymentTerms { Annual, Biannual, Quarterly, Monthly }
        public InsuranceType insType { get; set; }
        public InsuranceScope insScope { get; set; }
        public PaymentTerms insTerms { get; set; }
        public int InsuredAmount { get; set; }
        public double Deductible { get; set; }
        public int TermLength { get; set; }
        public double PaymentAmount { get; set; }
        public int CancellationFee { get; set; }
        public string PolicyIndex { get; set; }
        public DateTime InsuranceEffective { get; set; }
        public DateTime InsuranceExpires { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public int RemainingPayments { get; set; }
        public AirlinerInsurance(InsuranceType insType, InsuranceScope insScope, PaymentTerms paymentTerms, int insAmount)
        {
            this.Deductible = 0;
            this.TermLength = 0;
            this.CancellationFee = 0;
            this.InsuredAmount = insAmount;
        }

        //add insurance policy
        public static void CreatePolicy(Airline airline, FleetAirliner airliner, InsuranceType type, InsuranceScope scope, PaymentTerms terms, int length, int amount)
        {
            #region Method Setup
            Random rnd = new Random();
            double hub = airline.getHubs().Count() * 0.1;
            AirlinerInsurance policy = new AirlinerInsurance(type, scope, terms, amount);
            policy.InsuranceEffective = GameObject.GetInstance().GameTime;
            policy.InsuranceExpires = GameObject.GetInstance().GameTime.AddYears(length);
            policy.PolicyIndex = GameObject.GetInstance().GameTime.ToString() + airline.ToString();
            switch (policy.insTerms)
            {
                case PaymentTerms.Monthly:
                    policy.RemainingPayments = length * 12;
                    break;
                case PaymentTerms.Quarterly:
                    policy.RemainingPayments = length * 4;
                    break;
                case PaymentTerms.Biannual:
                    policy.RemainingPayments = length * 2;
                    break;
                case PaymentTerms.Annual:
                    policy.RemainingPayments = length;
                    break;
            }
            //sets up multipliers based on the type and scope of insurance policy
            Dictionary<InsuranceType, Double> typeMultipliers = new Dictionary<InsuranceType, double>();
            Dictionary<InsuranceScope, Double> scopeMultipliers = new Dictionary<InsuranceScope, double>();
            double typeMLiability = 1;
            double typeMGround_Parked = 1.2;
            double typeMGroundTaxi = 1.5;
            double typeMGroundCombined = 1.8;
            double typeMInFlight = 2.2;
            double typeMFullCoverage = 2.7;

            double scMAirport = 1;
            double scMDomestic = 1.5;
            double scMHub = 1.5 + hub;
            double scMGlobal = 2.0 + hub;
            #endregion
            #region Domestic/Int'l Airport Counter
            int i = 0; int j = 0;
            foreach (Airport airport in GameObject.GetInstance().HumanAirline.Airports)
            {
                if (airport.Profile.Country != GameObject.GetInstance().HumanAirline.Profile.Country)
                {
                    i++;
                }
                else j++;
            }
            #endregion
            // all the decision making for monthly payment amounts and deductibles
            switch (type)
            {
                #region Liability
                case InsuranceType.Liability:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMLiability * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;

                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMLiability * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMLiability * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMLiability * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Ground Parked

                case InsuranceType.Ground_Parked:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGround_Parked * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGround_Parked * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGround_Parked * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGround_Parked * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Ground Taxi
                case InsuranceType.Ground_Taxi:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundTaxi * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundTaxi * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundTaxi * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundTaxi * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Ground Combined
                case InsuranceType.Combined_Ground:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundCombined * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundCombined * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundCombined * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMGroundCombined * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region In Flight
                case InsuranceType.In_Flight:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMInFlight * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMInFlight * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMInFlight * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMInFlight * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                
                    break;
                #endregion
                #region Full Coverage
                case InsuranceType.Full_Coverage:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFullCoverage * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFullCoverage * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFullCoverage * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFullCoverage * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion

            }
        }

        public static void AddPolicy(FleetAirliner airliner, AirlinerInsurance insurance, string index)
        {
            airliner.InsurancePolicies.Add(index, insurance);
        }

        //remove insurance policy
        public static void RemovePolicy(FleetAirliner airliner, string index)
        {
            airliner.InsurancePolicies.Remove(index);
        }
       

        //extend or modify policy
        public static void ModifyPolicy(FleetAirliner airliner, string index, AirlinerInsurance newPolicy)
        {
            AirlinerInsurance oldPolicy = airliner.InsurancePolicies[index];
            //use the index to compare the new policy passed in to the existing one and make changes
        }

        public static void CheckExpiredInsurance(Airline airline)
        {
            DateTime date = GameObject.GetInstance().GameTime;
            foreach (FleetAirliner airliner in airline.Fleet)
            {
                foreach(AirlinerInsurance policy in airliner.InsurancePolicies.Values)
                {
                    if(policy.InsuranceExpires < GameObject.GetInstance().GameTime)
                    RemovePolicy(airliner, policy.PolicyIndex);
                }
            }
        }

        public static void MakeInsurancePayment(FleetAirliner airliner, Airline airline)
        {
            foreach (AirlinerInsurance policy in airliner.InsurancePolicies.Values)
            {
                if (policy.RemainingPayments > 0)
                {
                    if (policy.NextPaymentDue.Month == GameObject.GetInstance().GameTime.Month)
                    {
                        airline.Money -= policy.PaymentAmount;
                        Invoice payment = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, policy.PaymentAmount);
                        airline.addInvoice(payment);
                        policy.RemainingPayments--;
                        switch (policy.insTerms)
                        {
                            case PaymentTerms.Monthly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(1);
                                break;
                            case PaymentTerms.Quarterly:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(3);
                                break;
                            case PaymentTerms.Biannual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(6);
                                break;
                            case PaymentTerms.Annual:
                                policy.NextPaymentDue = GameObject.GetInstance().GameTime.AddMonths(12);
                                break;
                        }
                    }
                }

            }
        }

        public static void FileInsuranceClaim(Airline airline, Airport airport, AirportFacilities facility)
        {

        }

        public static void ReceiveInsurancePayout(Airline airline, Airport airport)
        {

        }
    }
}
