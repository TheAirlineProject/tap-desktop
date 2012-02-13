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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineOrdered.xaml
    /// </summary>
    public partial class PageAirlineOrdered : Page
    {
        private Airline Airline;
        public PageAirlineOrdered(Airline airline)
        {
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelOrdered = new StackPanel();
            panelOrdered.Margin = new Thickness(0, 10, 50, 0);

            ContentControl txtHeader = new ContentControl();
            txtHeader.ContentTemplate = this.Resources["OrderedHeader"] as DataTemplate;
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            
            panelOrdered.Children.Add(txtHeader);

            ListBox lbInOrder = new ListBox();
            lbInOrder.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbInOrder.ItemTemplate = this.Resources["InOrderItem"] as DataTemplate;
            lbInOrder.MaxHeight = 400;

            panelOrdered.Children.Add(lbInOrder);

            List<FleetAirliner> airliners = this.Airline.Fleet;

            foreach (FleetAirliner airliner in airliners.FindAll((delegate(FleetAirliner a) { return a.Airliner.BuiltDate > GameObject.GetInstance().GameTime; })))
                lbInOrder.Items.Add(airliner);

            this.Content = panelOrdered;
        }
    }
}
