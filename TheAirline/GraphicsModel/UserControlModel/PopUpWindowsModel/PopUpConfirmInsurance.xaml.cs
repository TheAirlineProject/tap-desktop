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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpConfirmInsurance.xaml
    /// </summary>
    public partial class PopUpConfirmInsurance : PopUpWindow
    {
        public AirlineInsurance Insurance { get; set; }
        public static object ShowPopUp(AirlineInsurance policy)
        {
            PopUpWindow window = new PopUpConfirmInsurance(policy);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpConfirmInsurance(AirlineInsurance policy)
        {
            this.Insurance = policy;
            this.DataContext = this.Insurance;

            InitializeComponent();

      
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = this.Insurance;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }
    }
    public class InsuranceTermsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            AirlineInsurance insurance = (AirlineInsurance)value;

            string termsType = "";

            if (insurance.InsTerms == AirlineInsurance.PaymentTerms.Annual)
                termsType = "per year";

            if (insurance.InsTerms == AirlineInsurance.PaymentTerms.Biannual)
                termsType = "per half year";

            if (insurance.InsTerms == AirlineInsurance.PaymentTerms.Quarterly)
                termsType = "per quarter";

            if (insurance.InsTerms == AirlineInsurance.PaymentTerms.Monthly)
                termsType = "per month";

            return string.Format("{0} {1}", new ValueCurrencyConverter().Convert(insurance.PaymentAmount), termsType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
