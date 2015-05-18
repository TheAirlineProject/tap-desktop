using TheAirline.Helpers;
using TheAirline.Models.Airlines;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for PageAirlineInsurance.xaml
    /// </summary>
    public partial class PageAirlineInsurance : Page
    {
        #region Constructors and Destructors

        public PageAirlineInsurance(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            this.InitializeComponent();

            this.Loaded += this.PageAirlineInsurance_Loaded;
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        #endregion

        #region Methods

        private void PageAirlineInsurance_Loaded(object sender, RoutedEventArgs e)
        {
            this.setValues();
        }

        //sets the values

        private void btnCreateInsurance_Click(object sender, RoutedEventArgs e)
        {
            var type = (AirlineInsurance.InsuranceType)this.cbType.SelectedItem;
            var scope = (AirlineInsurance.InsuranceScope)this.cbScope.SelectedItem;
            var terms = (AirlineInsurance.PaymentTerms)this.cbTerms.SelectedItem;
            Boolean allAirliners = this.cbAllAirliners.IsChecked.Value;
            int lenght = Convert.ToInt16(this.cbLength.SelectedItem);
            int amount = Convert.ToInt32(this.slAmount.Value);

            AirlineInsurance insurance = AirlineInsuranceHelpers.CreatePolicy(
                this.Airline.Airline,
                type,
                scope,
                terms,
                allAirliners,
                lenght,
                amount);
        }

        private void btnSetAdvertisement_Click(object sender, RoutedEventArgs e)
        {
            this.Airline.saveAdvertisements();
        }

        private void setValues()
        {
            this.cbType.Items.Clear();
            foreach (AirlineInsurance.InsuranceType type in Enum.GetValues(typeof(AirlineInsurance.InsuranceType)))
            {
                this.cbType.Items.Add(type);
            }
            this.cbType.SelectedIndex = 0;

            this.cbTerms.Items.Clear();
            foreach (AirlineInsurance.PaymentTerms term in Enum.GetValues(typeof(AirlineInsurance.PaymentTerms)))
            {
                this.cbTerms.Items.Add(term);
            }
            this.cbTerms.SelectedIndex = 0;

            this.cbScope.Items.Clear();
            foreach (AirlineInsurance.InsuranceScope scope in Enum.GetValues(typeof(AirlineInsurance.InsuranceScope)))
            {
                this.cbScope.Items.Add(scope);
            }
            this.cbScope.SelectedIndex = 0;

            this.cbLength.Items.Clear();
            for (int i = 0; i < 20; i++)
            {
                this.cbLength.Items.Add(i + 1);
            }
            this.cbLength.SelectedIndex = 0;

            foreach (AirlineAdvertisementMVVM advertisement in this.Airline.Advertisements)
            {
                advertisement.SelectedType = this.Airline.Airline.GetAirlineAdvertisement(advertisement.Type);
            }
        }

        #endregion
    }
}