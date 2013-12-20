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

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInsurance.xaml
    /// </summary>
    public partial class PageAirlineInsurance : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineInsurance(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
        
            InitializeComponent();

            this.Loaded += PageAirlineInsurance_Loaded;

      
        }

        private void PageAirlineInsurance_Loaded(object sender, RoutedEventArgs e)
        {
            setValues();
        }
        //sets the values
        private void setValues()
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

             cbLength.Items.Clear();
            for (int i = 0; i < 20; i++)
                cbLength.Items.Add(i + 1);
            cbLength.SelectedIndex = 0;

            foreach (AirlineAdvertisementMVVM advertisement in this.Airline.Advertisements)
                advertisement.SelectedType = this.Airline.Airline.getAirlineAdvertisement(advertisement.Type);

        }
        private void btnSetAdvertisement_Click(object sender, RoutedEventArgs e)
        {
            
            this.Airline.saveAdvertisements();
        
        
        }
        private void btnCreateInsurance_Click(object sender, RoutedEventArgs e)
        {
            AirlineInsurance.InsuranceType type = (AirlineInsurance.InsuranceType)cbType.SelectedItem;
            AirlineInsurance.InsuranceScope scope = (AirlineInsurance.InsuranceScope)cbScope.SelectedItem;
            AirlineInsurance.PaymentTerms terms = (AirlineInsurance.PaymentTerms)cbTerms.SelectedItem;
            Boolean allAirliners = cbAllAirliners.IsChecked.Value;
            int lenght = Convert.ToInt16(cbLength.SelectedItem);
            int amount = Convert.ToInt32(slAmount.Value);

            AirlineInsurance insurance = AirlineInsuranceHelpers.CreatePolicy(this.Airline.Airline, type, scope, terms, allAirliners, lenght, amount);

          }
      
    }
}
