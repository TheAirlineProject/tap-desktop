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
using TheAirline.GUIModel.HelpersModel;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    /// <summary>
    /// Interaction logic for PageAirline.xaml
    /// </summary>
    public partial class PageAirline : Page
    {
        private AirlineMVVM Airline;
        public PageAirline(Airline airline)
        {
            this.Airline = new AirlineMVVM(airline);
            this.Loaded += PageAirline_Loaded;

            InitializeComponent();
        }

        private void PageAirline_Loaded(object sender, RoutedEventArgs e)
        {
            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            frmContent.Navigate(new PageAirlineInfo(this.Airline){ Tag = this });
        }
        private void tcMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl control = (TabControl)sender;

            string selection = ((TabItem)control.SelectedItem).Tag.ToString();

            Frame frmContent = UIHelpers.FindChild<Frame>(this, "frmContent");

            if (selection == "Overview" && frmContent != null)
                frmContent.Navigate(new PageAirlineInfo(this.Airline) { Tag = this });

            if (selection == "Finances" && frmContent != null)
                frmContent.Navigate(new PageAirlineFinances(this.Airline) { Tag = this });

        }
    }
}
