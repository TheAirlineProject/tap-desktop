using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    //the mvvm object for an airline
    public class AirlineMVVM
    {
        public Airline Airline { get; set; }
        public List<FleetAirliner> DeliveredFleet { get; set; }
        public List<FleetAirliner> OrderedFleet { get; set; }
        public List<AirlineFinanceMVVM> Finances { get; set; }
        public double Balance { get; set; }
        public AirlineMVVM(Airline airline)
        {
            this.Airline = airline;
            this.DeliveredFleet = this.Airline.Fleet.FindAll(a=> a.Airliner.BuiltDate <= GameObject.GetInstance().GameTime);
            this.OrderedFleet = this.Airline.Fleet.FindAll(a=>a.Airliner.BuiltDate > GameObject.GetInstance().GameTime);
            this.Balance = this.Airline.Money - this.Airline.StartMoney;
            this.Finances = new List<AirlineFinanceMVVM>();

            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
                this.Finances.Add(new AirlineFinanceMVVM(airline, type));
        }
    }
    //the mvvm object for airline finances
    public class AirlineFinanceMVVM
    {
          public Invoice.InvoiceType InvoiceType { get; set; }
            public Airline Airline { get; set; }
            public double CurrentMonth { get { return getCurrentMonthTotal(); } set { ;} }
            public double LastMonth { get { return getLastMonthTotal(); } set { ;} }
            public double YearToDate { get { return getYearToDateTotal(); } set { ;} }
            public AirlineFinanceMVVM(Airline airline,Invoice.InvoiceType type)
            {
                this.InvoiceType = type;
                this.Airline = airline;
            }

            //returns the total amount for the current month
            public double getCurrentMonthTotal()
            {
                DateTime startDate = new DateTime(GameObject.GetInstance().GameTime.Year,GameObject.GetInstance().GameTime.Month,1);
                return this.Airline.getInvoicesAmount(startDate, GameObject.GetInstance().GameTime, this.InvoiceType);
            }

            //returns the total amount for the last month
            public double getLastMonthTotal()
            {
                DateTime tDate = GameObject.GetInstance().GameTime.AddMonths(-1);
                return this.Airline.getInvoicesAmountMonth(tDate.Year,tDate.Month, this.InvoiceType);
            }

            //returns the total amount for the year to date
            public double getYearToDateTotal()
            {
                return  this.Airline.getInvoicesAmountYear(GameObject.GetInstance().GameTime.Year, this.InvoiceType);
            }
        
    }
}
