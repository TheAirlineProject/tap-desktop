using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirportMap.xaml
    /// </summary>
    public partial class PopUpAirportMap : PopUpWindow
    {
        private Airport Airport;
        //shows the pop up for an airport
        public static void ShowPopUp(Airport airport)
        {
            PopUpAirportMap window = new PopUpAirportMap(airport);
            window.ShowDialog();
        }
        public PopUpAirportMap(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();
            this.Uid = "1000";
            
            this.Width = 900;

            this.Height = 450;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            Image imgMap = new Image();
            imgMap.Width = 900;
            imgMap.Height = 450;
            imgMap.Source = new BitmapImage(new Uri(this.Airport.Profile.Map, UriKind.RelativeOrAbsolute));
            RenderOptions.SetBitmapScalingMode(imgMap, BitmapScalingMode.HighQuality);

            this.Content = imgMap;

        }
    }
}
