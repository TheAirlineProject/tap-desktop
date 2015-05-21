using System.Collections.Generic;
using System.Linq;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Models.General;
using TheAirline.Models.Routes;

namespace TheAirline.GUIModel.CustomControlsModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpShowConnectingRoutes.xaml
    /// </summary>
    public partial class PopUpShowConnectingRoutes : PopUpWindow
    {
        public static void ShowPopUp(Airport airport1, Airport airport2)
        {
            PopUpWindow window = new PopUpShowConnectingRoutes(airport1,airport2); ;
            window.ShowDialog();

        }
        public PopUpShowConnectingRoutes(Airport destination1, Airport destination2)
        {
            List<Route> connectingRoutes = new List<Route>();

            var codesharingRoutes = GameObject.GetInstance().HumanAirline.Codeshares.Where(c => c.Airline2 == GameObject.GetInstance().HumanAirline || c.Type == CodeshareAgreement.CodeshareType.BothWays).Select(c => c.Airline1 == GameObject.GetInstance().HumanAirline ? c.Airline2 : c.Airline1).SelectMany(a => a.Routes);
            var humanConnectingRoutes = GameObject.GetInstance().HumanAirline.Routes.Where(r => r.Destination1 == destination1 || r.Destination2 == destination1 || r.Destination1 == destination2 || r.Destination2 == destination2);

            var codesharingConnectingRoutes = codesharingRoutes.Where(r => r.Destination1 == destination1 || r.Destination2 == destination1 || r.Destination1 == destination2 || r.Destination2 == destination2);

            foreach (Route route in humanConnectingRoutes)
                connectingRoutes.Add(route);

            foreach (Route route in codesharingConnectingRoutes)
                connectingRoutes.Add(route);

            this.DataContext = connectingRoutes;

            InitializeComponent();
        }
    }

}
