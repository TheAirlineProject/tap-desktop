using System;
using System.Runtime.Serialization;
using TheAirline.Model.GeneralModel;

namespace TheAirline.Model.AirlinerModel
{
    [Serializable]
    public class AirlinerInsurance : BaseModel
    {
        #region Constructors and Destructors

        public AirlinerInsurance(
            InsuranceType insType,
            InsuranceScope insScope,
            PaymentTerms paymentTerms,
            int insAmount)
        {
            Deductible = 0;
            TermLength = 0;
            CancellationFee = 0;
            InsuredAmount = insAmount;
        }

        private AirlinerInsurance(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum InsuranceScope
        {
            Airport,

            Domestic,

            Hub,

            Global
        }

        public enum InsuranceType
        {
            Liability,

            GroundParked,

            GroundTaxi,

            CombinedGround,

            InFlight,

            FullCoverage
        }

        public enum PaymentTerms
        {
            Annual,

            Biannual,

            Quarterly,

            Monthly
        }

        #endregion

        #region Public Properties

        [Versioning("cancellation")]
        public int CancellationFee { get; set; }

        [Versioning("deductible")]
        public double Deductible { get; set; }

        [Versioning("insscope")]
        public InsuranceScope InsScope { get; set; }

        [Versioning("instype")]
        public InsuranceType InsType { get; set; }

        [Versioning("insuranceeffective")]
        public DateTime InsuranceEffective { get; set; }

        [Versioning("insuranceexpires")]
        public DateTime InsuranceExpires { get; set; }

        [Versioning("insuredamount")]
        public int InsuredAmount { get; set; }

        [Versioning("nextpaymentdue")]
        public DateTime NextPaymentDue { get; set; }

        [Versioning("paymentamount")]
        public double PaymentAmount { get; set; }

        [Versioning("policyindex")]
        public string PolicyIndex { get; set; }

        [Versioning("remainingpayments")]
        public double RemainingPayments { get; set; }

        [Versioning("termlength")]
        public int TermLength { get; set; }

        [Versioning("insterms")]
        public PaymentTerms InsTerms { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}