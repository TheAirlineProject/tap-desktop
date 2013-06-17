using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.InvoicesModel
{
    //the class for a monthly invoice
    [ProtoContract]
    public class MonthlyInvoice
    {
        [ProtoMember(1)]
        public Invoice.InvoiceType Type { get; set; }
        [ProtoMember(2)]
        public double Amount { get; set; }
        [ProtoMember(3)]
        public int Year { get; set; }
        [ProtoMember(4)]
        public int Month { get; set; }
        public MonthlyInvoice(Invoice.InvoiceType type, int year, int month, double amount)
        {
            this.Type = type;
            this.Year = year;
            this.Month = month;
            this.Amount = amount;
        }
        public MonthlyInvoice(Invoice.InvoiceType type, int year, int month)
            : this(type, year, month,0)
        {
        }
    }
}
