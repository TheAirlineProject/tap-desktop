using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheAirline.Model.GeneralModel.InvoicesModel
{
    //the class for a collection of invoices
    public class Invoices
    {
        public List<MonthlyInvoice> MonthlyInvoices { get; set; }
        public Invoices()
        {
            this.MonthlyInvoices = new List<MonthlyInvoice>();
        }
        //adds an invoice to the invoices
        public void addInvoice(Invoice invoice)
        {
            addInvoice(invoice.Type, invoice.Date.Year, invoice.Date.Month, invoice.Amount);            
        }
        public void addInvoice(Invoice.InvoiceType type, int year, int month, double amount)
        {
         
            if (contains(type, year, month))
            {
                MonthlyInvoice mInvoice = this.MonthlyInvoices.Find(m => m.Month == month && m.Year == year && m.Type == type);
                mInvoice.Amount += amount;
            }
            else
            {
                MonthlyInvoice mInvoice = new MonthlyInvoice(type, year, month, amount);
                this.MonthlyInvoices.Add(mInvoice);
            }
        }
        //returns the yearly amount for a given type and year
        public double getYearlyAmount(Invoice.InvoiceType type, int year)
        {
            return this.MonthlyInvoices.FindAll(i => i.Type == type && i.Year == year).Sum(i => i.Amount);
        }
        //return the yearly amount for an year
        public double getYearlyAmount(int year)
        {
            return this.MonthlyInvoices.FindAll(i => i.Year == year).Sum(i => i.Amount);
        }
        //returns the total amount for a given type
        public double getAmount(Invoice.InvoiceType type)
        {
            return this.MonthlyInvoices.FindAll(i => i.Type == type).Sum(i => i.Amount);
        }
        //returns the total amount for a given year, month and type
        public double getAmount(Invoice.InvoiceType type, int year, int month)
        {
            if (contains(type, year, month))
            {
                MonthlyInvoice mInvoice = this.MonthlyInvoices.Find(m => m.Month == month && m.Year == year && m.Type == type);
                return mInvoice.Amount;
            }
            else
                return 0;
        }
        //returns the total amount for a given year and month
        public double getAmount(int year, int month)
        {
            return this.MonthlyInvoices.FindAll(m => m.Month == month && m.Year == year).Sum(m=>m.Amount);
        }
        //returns the total amount of invoices
        public double getAmount()
        {
            return this.MonthlyInvoices.Sum(m => m.Amount);
        }
        //returns if the invoices contains a month, year and type element
        public Boolean contains(Invoice.InvoiceType type,int year, int month)
        {
            return this.MonthlyInvoices.Exists(m=>m.Month == month && m.Year == year && m.Type == type);
        }
    }
}
