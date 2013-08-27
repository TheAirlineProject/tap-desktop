using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFinances.xaml
    /// </summary>
    public partial class PageAirlineFinances : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineFinances(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            InitializeComponent();
        }

        private void btnApplyLoan_Click(object sender, RoutedEventArgs e)
        {
            double amount = slAmount.Value;
            int length = Convert.ToInt16(slLenght.Value)*12;

            Loan loan = new Loan(GameObject.GetInstance().GameTime, amount, length, this.Airline.LoanRate);
            this.Airline.addLoan(loan);
            
            AirlineHelpers.AddAirlineInvoice(this.Airline.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Loans, loan.Amount);
            


        }
    }
}
