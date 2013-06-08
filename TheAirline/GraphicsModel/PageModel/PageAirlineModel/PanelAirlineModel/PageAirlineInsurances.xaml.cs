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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;

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
            for (int i = 10000; i < 100000; i += 10000)
                cbAmount.Items.Add(GeneralHelpers.GetInflationPrice(i));
            cbAmount.SelectedIndex= 0;

            cbAllAirliners.IsChecked = false;
            ucLength.Value = 1;

            lbInsurances.ItemsSource = null;
            lbInsurances.ItemsSource = this.Airline.Insurances;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AirlineInsurance.InsuranceType type = (AirlineInsurance.InsuranceType)cbType.SelectedItem;
            AirlineInsurance.InsuranceScope scope = (AirlineInsurance.InsuranceScope)cbScope.SelectedItem;
            AirlineInsurance.PaymentTerms terms = (AirlineInsurance.PaymentTerms)cbTerms.SelectedItem;
            Boolean allAirliners = cbAllAirliners.IsChecked.Value;
            int lenght = Convert.ToInt16(ucLength.Value);
            int amount = Convert.ToInt32(cbAmount.SelectedItem);
           
            
            AirlineInsurance policy = AirlineInsurance.CreatePolicy(this.Airline,type,scope,terms,allAirliners,lenght,amount);
            this.Airline.addInsurance(policy);

            clearValues();
          
        }
        
    }
}
