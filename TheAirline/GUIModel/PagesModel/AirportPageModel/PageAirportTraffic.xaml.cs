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

namespace TheAirline.GUIModel.PagesModel.AirportPageModel
{
    /// <summary>
    /// Interaction logic for PageAirportTraffic.xaml
    /// </summary>
    public partial class PageAirportTraffic : Page
    {
        public AirportMVVM Airport { get; set; }
        public PageAirportTraffic(AirportMVVM airport)
        {
            this.Airport = airport;
            this.DataContext = this.Airport;

            InitializeComponent();

            CollectionView viewTraffic = (CollectionView)CollectionViewSource.GetDefaultView(lvTraffic.ItemsSource);
            viewTraffic.GroupDescriptions.Clear();

            viewTraffic.GroupDescriptions.Add(new PropertyGroupDescription("Type"));

        }
    }
}
