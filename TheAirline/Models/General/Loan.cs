using System;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Helpers;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General
{
    //the class for a loan
    [Serializable]
    public class Loan : BaseModel
    {
        #region Constructors and Destructors

        public Loan(DateTime date, double amount, int length, double rate)
        {
            Amount = amount;
            Rate = rate;
            Length = length;
            Date = date;
            PaymentLeft = amount;
        }

        private Loan(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("amount")]
        public double Amount { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        public bool IsActive => HasPaymentLeft();

        [Versioning("length")]
        public int Length { get; set; }

        public double MonthlyPayment => GetMonthlyPayment();

        public int MonthsLeft => GetMonthsLeft();

        [Versioning("paymentleft")]
        public double PaymentLeft { get; set; }

        [Versioning("rate")]
        public double Rate { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        //returns the monthly payment for the loan

        public double GetMonthlyPayment()
        {
            double basePayment = MathHelpers.GetMonthlyPayment(Amount, Rate, Length);

            return basePayment*GameObject.GetInstance().Difficulty.LoanLevel;
        }

        #endregion

        #region Methods

        private int GetMonthsLeft()
        {
            return (int) Math.Ceiling(PaymentLeft/MonthlyPayment);
        }

        private bool HasPaymentLeft()
        {
            return PaymentLeft > 0;
        }

        #endregion

        //checks if there is still payment left on the loan

        //returns the amount of months left on the loan
    }
}