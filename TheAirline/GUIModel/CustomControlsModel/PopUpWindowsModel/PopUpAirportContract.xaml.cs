using System;
using System.Collections.Generic;
using System.Globalization;
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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirportContract.xaml
    /// </summary>
    public partial class PopUpAirportContract : PopUpWindow
    {
        public Airport Airport { get; set; }
        private AirportContract.ContractType SelectedType;
        public static object ShowPopUp(Airport airport)
        {
            PopUpWindow window = new PopUpAirportContract(airport);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirportContract(Airport airport)
        {
            this.Airport = airport;

            this.DataContext = this.Airport;

            InitializeComponent();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = this.SelectedType;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }

        private void rbContractType_Checked(object sender, RoutedEventArgs e)
        {
            string type = ((RadioButton)sender).Tag.ToString();
            this.SelectedType = (AirportContract.ContractType)Enum.Parse(typeof(AirportContract.ContractType), type, true);
        }
    }
    //the converter for the price of an airport contract
    public class AirportContractPriceConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Airport airport = (Airport)values[0];
            AirportContract.ContractType type = (AirportContract.ContractType)Enum.Parse(typeof(AirportContract.ContractType), values[1].ToString(), true);

            double price = AirportHelpers.GetYearlyContractPayment(airport,type, 2, 2);

            return string.Format("({0})",new ValueCurrencyConverter().Convert(price));

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
