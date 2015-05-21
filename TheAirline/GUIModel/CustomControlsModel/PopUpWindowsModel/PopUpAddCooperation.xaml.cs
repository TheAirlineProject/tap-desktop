using System.Collections.Generic;
using System.Windows;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airlines.AirlineCooperation;
using TheAirline.Models.Airports;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAddCooperation.xaml
    /// </summary>
    public partial class PopUpAddCooperation : PopUpWindow
    {
        private Airline Airline;
        private Airport Airport;
        public List<CooperationType> Types { get; set; }
        public static object ShowPopUp(Airline airline,Airport airport)
        {
            PopUpWindow window = new PopUpAddCooperation(airline,airport);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAddCooperation(Airline airline, Airport airport)
        {
            this.Types = new List<CooperationType>();
            
            this.Airline = airline;
            this.Airport = airport;

            foreach (CooperationType type in CooperationTypes.GetCooperationTypes())
            {
                if (!this.Airport.Cooperations.Exists(c => c.Airline == airline && c.Type == type) && type.AirportSizeRequired<=this.Airport.Profile.Size)
                    this.Types.Add(type);
            }

            InitializeComponent();

        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = cbCooperationType.SelectedItem;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
