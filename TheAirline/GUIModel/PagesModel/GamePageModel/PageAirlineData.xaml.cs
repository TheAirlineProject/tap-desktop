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

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    /// <summary>
    /// Interaction logic for PageAirlineData.xaml
    /// </summary>
    public partial class PageAirlineData : Page
    {
        public PageAirlineData()
        {
            InitializeComponent();

            cbAirline.ItemsSource = Airlines.GetAllAirlines();
        }

        private void cbAirline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Airline airline = (Airline)cbAirline.SelectedItem;

            txtNarrative.Text = airline.Profile.Narrative; 
        }
    }
}
