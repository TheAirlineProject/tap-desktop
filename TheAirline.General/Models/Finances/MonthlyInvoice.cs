using System;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Finances
{
    //the class for a monthly invoice
    [Serializable]
    public class MonthlyInvoice : BaseModel
    {
        #region Constructors and Destructors

        public MonthlyInvoice(Invoice.InvoiceType type, int year, int month, int day, double amount)
        {
            Type = type;
            Year = year;
            Month = month;
            Amount = amount;
            Day = day;
        }

        public MonthlyInvoice(Invoice.InvoiceType type, int year, int month, int day)
            : this(type, year, month, day, 0)
        {
        }

        private MonthlyInvoice(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
            if (Version == 1)
                Day = 1;
        }

        #endregion

        #region Public Properties

        [Versioning("amount")]
        public double Amount { get; set; }

        [Versioning("month")]
        public int Month { get; set; }

        [Versioning("type")]
        public Invoice.InvoiceType Type { get; set; }

        [Versioning("year")]
        public int Year { get; set; }

        [Versioning("day", Version = 2)]
        public int Day { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}