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
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportGateSchedule.xaml
    /// </summary>
    public partial class PageAirportGateSchedule : Page
    {
        public AirportMVVM Airport { get; set; }
        public PageAirportGateSchedule(AirportMVVM airport)
        {
            this.Airport = airport;

            this.DataContext = this.Airport;

            InitializeComponent();
        }
    }
}
