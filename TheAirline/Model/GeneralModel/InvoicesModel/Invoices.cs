
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TheAirline.Model.GeneralModel.InvoicesModel
{
    //the class for a collection of invoices
    [Serializable]
    public class Invoices : ISerializable
    {
        [Versioning("invoices")]
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
            lock (this.MonthlyInvoices)
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
            Boolean contains;
            lock (this.MonthlyInvoices)
            {
                contains = this.MonthlyInvoices.Exists(m => m.Month == month && m.Year == year && m.Type == type);
            }

            return contains;
        }

        public static List<double> getInvoiceList()
        {
            List<double> invoices = new List<double>();
            foreach (MonthlyInvoice invoice in GameObject.GetInstance().HumanAirline.getInvoices().MonthlyInvoices)
            {
                invoices.Add(invoice.Amount);
            }

            return invoices;

        }
             private Invoices(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }

        }
    }
}
