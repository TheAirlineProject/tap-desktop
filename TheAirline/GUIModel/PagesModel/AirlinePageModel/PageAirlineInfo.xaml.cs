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
using TheAirline.Model.AirlineModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInfo.xaml
    /// </summary>
    public partial class PageAirlineInfo : Page
    {
        public AirlineMVVM Airline { get; set; }
        public PageAirlineInfo(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            InitializeComponent();

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvFleet.ItemsSource);
            view.GroupDescriptions.Clear();

            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Purchased");
            view.GroupDescriptions.Add(groupDescription);
   
        }
    }
}
