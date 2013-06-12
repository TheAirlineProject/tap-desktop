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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInsurances.xaml
    /// </summary>
    public partial class PageAirlineInsurances : Page
    {
        public Airline Airline { get; set; }
        public PageAirlineInsurances(Airline airline)
        {
            
            this.Airline = airline;
         
            InitializeComponent();

            this.DataContext = this.Airline;

            clearValues();      
          

        }
        //clears the values of the boxes
        private void clearValues()
        {
            cbType.Items.Clear();
            foreach (AirlineInsurance.InsuranceType type in Enum.GetValues(typeof(AirlineInsurance.InsuranceType)))
                cbType.Items.Add(type);
            cbType.SelectedIndex = 0;

            cbTerms.Items.Clear();
            foreach (AirlineInsurance.PaymentTerms term in Enum.GetValues(typeof(AirlineInsurance.PaymentTerms)))
                cbTerms.Items.Add(term);
            cbTerms.SelectedIndex = 0;

            cbScope.Items.Clear();
            foreach (AirlineInsurance.InsuranceScope scope in Enum.GetValues(typeof(AirlineInsurance.InsuranceScope)))
                cbScope.Items.Add(scope);
            cbScope.SelectedIndex = 0;

            cbAmount.Items.Clear();
            for (int i = 500000; i < 1000000000; i += 500000)
                cbAmount.Items.Add(GeneralHelpers.GetInflationPrice(i));
            cbAmount.SelectedIndex= 0;

            cbAllAirliners.IsChecked = false;
            cbAllAirliners.Click += new RoutedEventHandler(cbAllAirliners_onClick);
            ucLength.Value = 1;

            lbInsurances.ItemsSource = null;
            lbInsurances.ItemsSource = this.Airline.InsurancePolicies;
        }

        private void cbAllAirliners_onClick(object sender, RoutedEventArgs e)
        {
            cbAmount.Items.Refresh();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AirlineInsurance.InsuranceType type = (AirlineInsurance.InsuranceType)cbType.SelectedItem;
            AirlineInsurance.InsuranceScope scope = (AirlineInsurance.InsuranceScope)cbScope.SelectedItem;
            AirlineInsurance.PaymentTerms terms = (AirlineInsurance.PaymentTerms)cbTerms.SelectedItem;
            Boolean allAirliners = cbAllAirliners.IsChecked.Value;
            int lenght = Convert.ToInt16(ucLength.Value);
            int amount = Convert.ToInt32(cbAmount.SelectedItem);

            AirlineInsurance insurance = AirlineInsuranceHelpers.CreatePolicy(this.Airline, type, scope, terms, allAirliners, lenght, amount);

            if (PopUpConfirmInsurance.ShowPopUp(insurance) != null)
                this.Airline.addInsurance(insurance);
          
            clearValues();
          
        }
        
    }
}
