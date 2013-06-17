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

namespace TheAirline.Model.AirlinerModel
{
    [ProtoContract]
    public class AirlinerInsurance
    {
        public enum InsuranceType { Liability, Ground_Parked, Ground_Taxi,Combined_Ground, In_Flight, Full_Coverage }
        public enum InsuranceScope { Airport, Domestic, Hub, Global }
        public enum PaymentTerms { Annual, Biannual, Quarterly, Monthly }
        [ProtoMember(1)]
        public InsuranceType InsType { get; set; }
        [ProtoMember(2)]
        public InsuranceScope InsScope { get; set; }
        [ProtoMember(3)]
        public PaymentTerms insTerms { get; set; }
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
        public DateTime InsuranceEffective { get; set; }
        [ProtoMember(11)]
        public DateTime InsuranceExpires { get; set; }
        [ProtoMember(12)]
        public DateTime NextPaymentDue { get; set; }
        [ProtoMember(13)]
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
