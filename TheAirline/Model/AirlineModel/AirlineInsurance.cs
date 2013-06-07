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
        public enum InsuranceType { None, Public_Liability, Passenger_Liability, Combined_Single_Limit, Full_Coverage }
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
        }

        //add insurance policy
        public static AirlineInsurance CreatePolicy(Airline airline, InsuranceType type, InsuranceScope scope, PaymentTerms terms, bool allAirliners, int length, int amount)
        {
            #region Method Setup
            Random rnd = new Random();
            double modifier = GetRatingModifier(airline);
            double hub = airline.getHubs().Count() * 0.1;
            AirlineInsurance policy = new AirlineInsurance(type, scope, terms, amount);
            policy.InsuranceEffective = GameObject.GetInstance().GameTime;
            policy.InsuranceExpires = GameObject.GetInstance().GameTime.AddYears(length);
            policy.PolicyIndex = GameObject.GetInstance().GameTime.ToString() + airline.ToString();
            switch (policy.InsTerms)
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
            double typeMPublic = modifier;
            double typeMPassenger = modifier + 0.2;
            double typeMCSL = modifier + 0.5;
            double typeMFull = modifier + 1;

            double scMAirport = modifier;
            double scMDomestic = modifier + 0.2;
            double scMHub = modifier + hub + 0.5;
            double scMGlobal = modifier + hub + 1;
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
            #region Public Liability
            switch (type)
            {
                case InsuranceType.Public_Liability:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;

                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPublic * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;

            #endregion
                #region Passenger Liability

                case InsuranceType.Passenger_Liability:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMPassenger * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                    break;
                #endregion
                #region Combined Single Limit
                case InsuranceType.Combined_Single_Limit:
                    switch (scope)
                    {
                        case InsuranceScope.Airport:
                            policy.Deductible = amount * 0.005;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMCSL * scMGlobal;
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
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMAirport;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Domestic:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMDomestic;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Hub:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMHub;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;

                        case InsuranceScope.Global:
                            policy.Deductible = amount * 0.001;
                            policy.PaymentAmount = policy.InsuredAmount * (4 / 10) * typeMFull * scMGlobal;
                            if (terms == PaymentTerms.Annual) policy.PaymentAmount = policy.InsuredAmount / length;
                            if (terms == PaymentTerms.Biannual) policy.PaymentAmount = policy.InsuredAmount / length / 2;
                            if (terms == PaymentTerms.Quarterly) policy.PaymentAmount = policy.InsuredAmount / length / 4;
                            if (terms == PaymentTerms.Monthly) policy.PaymentAmount = policy.InsuredAmount / length / 12;
                            break;
                    }
                #endregion
                    break;
            }

            if (allAirliners == true)
            {
                amount *= airline.Fleet.Count();
               // PaymentAmount *= (airline.Fleet.Count() * 0.95);
            }
            return policy;
        }

       

        //gets insurance rate modifiers based on security, safety, and aircraft state of maintenance
        public static double GetRatingModifier(Airline airline)
        {
            double mod = 1;
            mod += (100 - airline.Ratings.MaintenanceRating) / 100;
            mod += (100 - airline.Ratings.SafetyRating) / 150;
            mod += (100 - airline.Ratings.SecurityRating) / 100;
            return mod;
        }

        
        //extend or modify policy
        public static void ModifyPolicy(Airline airline, string index, AirlineInsurance newPolicy)
        {
            AirlineInsurance oldPolicy = airline.InsurancePolicies[index];
            //use the index to compare the new policy passed in to the existing one and make changes
        }

        public static void CheckExpiredInsurance(Airline airline)
        {
            DateTime date = GameObject.GetInstance().GameTime;
            foreach (AirlineInsurance policy in airline.InsurancePolicies.Values)
            {
                if (policy.InsuranceExpires < date)
                {
                    airline.removeInsurance(policy);
                }
            }
        }

        public static void MakeInsurancePayment(Airline airline)
        {
            foreach (AirlineInsurance policy in airline.InsurancePolicies.Values)
            {
                if (policy.RemainingPayments > 0)
                {
                    if (policy.NextPaymentDue.Month == GameObject.GetInstance().GameTime.Month)
                    {
                        airline.Money -= policy.PaymentAmount;
                        Invoice payment = new Invoice(GameObject.GetInstance().GameTime, Invoice.InvoiceType.Maintenances, policy.PaymentAmount);
                        airline.addInvoice(payment);
                        policy.RemainingPayments--;
                        switch (policy.InsTerms)
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


        //for general damage or claims
        public static void FileInsuranceClaim(Airline airline, AirlineInsurance policy, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, null, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
            
        }

        //for damage and claims involving an airport or airport facility
        public static void FileInsuranceClaim(Airline airline, Airport airport, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, null, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }


        //for damage and claims involving an airliner
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, airliner, null, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }

        //for damage and claims involving an airliner and airport or airport facility
        public static void FileInsuranceClaim(Airline airline, FleetAirliner airliner, Airport airport, int damage)
        {
            InsuranceClaim claim = new InsuranceClaim(airline, airliner, airport, GameObject.GetInstance().GameTime, damage);
            airline.InsuranceClaims.Add(claim);
            News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Filed", "You have filed an insurance claim. Reference: " + claim.Index);
        }
        

        public static void ReceiveInsurancePayout(Airline airline, AirlineInsurance policy, InsuranceClaim claim)
        {
            if (claim.Damage > policy.Deductible)
            {
                claim.Damage -= (int)policy.Deductible;
                airline.Money -= policy.Deductible;
                policy.Deductible = 0;
                policy.InsuredAmount -= claim.Damage;
                airline.Money += claim.Damage;
                News news = new News(News.NewsType.Airline_News, GameObject.GetInstance().GameTime, "Insurance Claim Payout", "You have received an insurance payout in the amount of $" + claim.Damage.ToString() + ". This was for claim number " + claim.Index);
            }

            else if (claim.Damage < policy.Deductible)
            {
                policy.Deductible -= claim.Damage;
                GraphicsModel.PageModel.GeneralModel.Warnings.AddWarning("Low Damage", "The damage incurred was less than your deductible, so you will not receive an insurance payout for this claim! \n Reference: " + claim.Index);
            }
        }
    }

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
