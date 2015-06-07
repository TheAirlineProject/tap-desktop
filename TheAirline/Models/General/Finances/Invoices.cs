using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TheAirline.General.Models;
using TheAirline.Infrastructure;

namespace TheAirline.Models.General.Finances
{
    //the class for a collection of invoices
    [Serializable]
    public class Invoices : BaseModel
    {
        #region Constructors and Destructors

        public Invoices()
        {
            MonthlyInvoices = new List<MonthlyInvoice>();
        }

        private Invoices(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        {
        }

        #endregion

        #region Public Properties

        [Versioning("invoices")]
        public List<MonthlyInvoice> MonthlyInvoices { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            base.GetObjectData(info, context);
        }

        public static List<double> GetInvoiceList()
        {
            return GameObject.GetInstance().HumanAirline.GetInvoices().MonthlyInvoices.Select(invoice => invoice.Amount).ToList();
        }

        //adds an invoice to the invoices
        public void AddInvoice(Invoice invoice)
        {
            AddInvoice(invoice.Type, invoice.Date.Year, invoice.Date.Month, invoice.Date.Day, invoice.Amount);
        }

        public void AddInvoice(Invoice.InvoiceType type, int year, int month, int day, double amount)
        {
            lock (MonthlyInvoices)
            {
                if (Contains(type, year, month, day))
                {
                    MonthlyInvoice mInvoice =
                        MonthlyInvoices.Find(m => m.Month == month && m.Year == year && m.Type == type && m.Day == day);
                    mInvoice.Amount += amount;
                }
                else
                {
                    var mInvoice = new MonthlyInvoice(type, year, month, day, amount);
                    MonthlyInvoices.Add(mInvoice);
                }
            }
        }

        public Boolean Contains(Invoice.InvoiceType type, int year, int month)
        {
            Boolean contains;
            lock (MonthlyInvoices)
            {
                contains = MonthlyInvoices.Exists(m => m.Month == month && m.Year == year && m.Type == type);
            }

            return contains;
        }

        public Boolean Contains(Invoice.InvoiceType type, int year, int month, int day)
        {
            Boolean contains;
            lock (MonthlyInvoices)
            {
                contains = MonthlyInvoices.Exists(m => m.Month == month && m.Year == year && m.Day == day && m.Type == type);
            }

            return contains;
        }

        //returns the yearly amount for a given type and year

        //returns the total amount for a given type
        public double GetAmount(Invoice.InvoiceType type)
        {
            return MonthlyInvoices.FindAll(i => i.Type == type).Sum(i => i.Amount);
        }

        //returns the total amount for a given year, month and type
        public double GetAmount(Invoice.InvoiceType type, int year, int month)
        {
            if (Contains(type, year, month))
            {
                MonthlyInvoice mInvoice =
                    MonthlyInvoices.Find(m => m.Month == month && m.Year == year && m.Type == type);
                return mInvoice.Amount;
            }
            return 0;
        }

        //returns the total amount for a given year and month
        public double GetAmount(int year, int month)
        {
            return MonthlyInvoices.FindAll(m => m.Month == month && m.Year == year).Sum(m => m.Amount);
        }

        //returns the total amount of invoices
        public double GetAmount()
        {
            return MonthlyInvoices.Sum(m => m.Amount);
        }

        public double GetYearlyAmount(Invoice.InvoiceType type, int year)
        {
            return MonthlyInvoices.FindAll(i => i.Type == type && i.Year == year).Sum(i => i.Amount);
        }

        //return the yearly amount for an year
        public double GetYearlyAmount(int year)
        {
            return MonthlyInvoices.FindAll(i => i.Year == year).Sum(i => i.Amount);
        }

        #endregion

        //returns if the invoices contains a month, year and type element
    }
}