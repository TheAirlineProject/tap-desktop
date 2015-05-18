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
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Helpers;
using TheAirline.Model.GeneralModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.General;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpCodeshareAgreement.xaml
    /// </summary>
    public partial class PopUpCodeshareAgreement : PopUpWindow
    {
        private Airline Airline;
        public double Price { get; set; }
        public double TicketSalePercent { get; set; }
        public static object ShowPopUp(Airline airline)
        {
            PopUpWindow window = new PopUpCodeshareAgreement(airline);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpCodeshareAgreement(Airline airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;
            this.Price = AirlineHelpers.GetCodesharingPrice(this.Airline,GameObject.GetInstance().HumanAirline);
            this.TicketSalePercent = CodeshareAgreement.TicketSalePercent;

            InitializeComponent();
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = rbBothWays.IsChecked.Value ? CodeshareAgreement.CodeshareType.BothWays : CodeshareAgreement.CodeshareType.OneWay;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
