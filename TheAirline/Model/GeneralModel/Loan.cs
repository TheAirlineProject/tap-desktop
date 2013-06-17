using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a loan
    [ProtoContract]
    public class Loan
    {
        [ProtoMember(1)]
        public double Amount { get; set; }
        [ProtoMember(2)]
        public double Rate { get; set; }
        [ProtoMember(3)]
        public int Length { get; set; }
        [ProtoMember(4)]
        public DateTime Date { get; set; }
        [ProtoMember(5)]
        public double PaymentLeft { get; set; }
        public Boolean IsActive { get { return hasPaymentLeft(); } set { ;} }
        public double MonthlyPayment { get { return getMonthlyPayment(); } set { ;} }
        public int MonthsLeft { get { return getMonthsLeft(); } set { ;} }
        public Loan(DateTime date, double amount, int length, double rate)
        {
            this.Amount = amount;
            this.Rate = rate;
            this.Length = length;
            this.Date = date;
            this.PaymentLeft = amount;
            
        }
        //returns the monthly payment for the loan
       
        public double getMonthlyPayment()
        {
           double basePayment = MathHelpers.GetMonthlyPayment(this.Amount, this.Rate, this.Length);
                      
           return basePayment * GameObject.GetInstance().Difficulty.LoanLevel;
          
        }
        //checks if there is still payment left on the loan
        private Boolean hasPaymentLeft()
        {
            return this.PaymentLeft > 0;
        }
        //returns the amount of months left on the loan
        private int getMonthsLeft()
        {
            return (int)Math.Ceiling(this.PaymentLeft / this.MonthlyPayment);
        }
    }
}
