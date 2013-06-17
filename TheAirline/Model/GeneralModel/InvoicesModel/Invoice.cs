using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for an invoice
    [ProtoContract]
    public class Invoice
    {
        public enum InvoiceType { Wages, Rents, Loans, Purchases, Tickets, Airline_Expenses, Fees, Maintenances, Flight_Expenses, OnFlight_Income, Total }
        [ProtoMember(1)]
        public DateTime Date { get; set; }
        [ProtoMember(2)]
        public double Amount { get; set; }
        [ProtoMember(3)]
        public InvoiceType Type { get; set; }
        public Invoice(DateTime date, InvoiceType type, double amount)
        
    {
            this.Amount = amount;
            this.Type = type;
            this.Date = date;
            }

        
    }
}
