
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.InvoicesModel
{
    //the class for a monthly invoice
    [Serializable]
    public class MonthlyInvoice
    {
        
        public Invoice.InvoiceType Type { get; set; }
        
        public double Amount { get; set; }
        
        public int Year { get; set; }
        
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
