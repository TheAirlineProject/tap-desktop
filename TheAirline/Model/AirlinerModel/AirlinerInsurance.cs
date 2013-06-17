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
        public enum InsuranceType { Liability, Ground_Parked, Ground_Taxi,Combined_Ground, In_Flight, Full_Coverage }
        public enum InsuranceScope { Airport, Domestic, Hub, Global }
        public enum PaymentTerms { Annual, Biannual, Quarterly, Monthly }
        
        public InsuranceType InsType { get; set; }
        
        public InsuranceScope InsScope { get; set; }
        
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

     
    }
}
