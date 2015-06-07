using System;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Finances
{
    //the class for an invoice
    [Serializable]
    public class Invoice : BaseModel
    {
        #region Constructors and Destructors

        public Invoice(DateTime date, InvoiceType type, double amount)

        {
            Amount = amount;
            Type = type;
            Date = date;
        }

        private Invoice(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Enums

        public enum InvoiceType
        {
            Wages,

            Rents,

            Loans,

            Purchases,

            Tickets,

            AirlineExpenses,

            Fees,

            Maintenances,

            FlightExpenses,

            OnFlightIncome,

            Total
        }

        #endregion

        #region Public Properties

        [Versioning("amount")]
        public double Amount { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("type")]
        public InvoiceType Type { get; set; }

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