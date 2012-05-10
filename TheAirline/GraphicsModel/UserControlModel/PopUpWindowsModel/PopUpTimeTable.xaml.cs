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
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpTimeTable.xaml
    /// </summary>
    public partial class PopUpTimeTable : PopUpWindow
    {
        private Route Route;
        public static object ShowPopUp(Route route)
        {
            PopUpTimeTable window = new PopUpTimeTable(route);
            window.ShowDialog();

            return window.Selected == null ? null : window.Selected;
        }
        public PopUpTimeTable(Route route)
        {

            InitializeComponent();

            this.Uid = "1000";
            this.Route = route;



            this.Title = string.Format("{0} - {1}", this.Route.Destination1.Profile.IATACode, this.Route.Destination2.Profile.IATACode);

            this.Width = 800;

            this.Height = 450;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel panelMain = new StackPanel();
            panelMain.Margin = new Thickness(10, 10, 10, 10);

            Grid grdMain = UICreator.CreateGrid(2);

            StackPanel panelOutbound = createDestinationPanel(route.Destination1);
            Grid.SetColumn(panelOutbound, 0);
            grdMain.Children.Add(panelOutbound);

            StackPanel panelInbound = createDestinationPanel(route.Destination2);
            Grid.SetColumn(panelInbound, 1);
            grdMain.Children.Add(panelInbound);

            panelMain.Children.Add(grdMain);

            panelMain.Children.Add(createCPUButtonsPanel());
            
            this.Content = panelMain;

            showEntries();

        }
        //create the panel for a destination
        private StackPanel createDestinationPanel(Airport airport)
        {
            StackPanel destPanel = new StackPanel();

            destPanel.Margin = new Thickness(5, 0, 5, 0);
            destPanel.Children.Add(createHeader(airport));

            ListBox lbEntries = new ListBox();
            lbEntries.ItemTemplate = this.Resources["RouteTimeTableEntryItem"] as DataTemplate;
            lbEntries.Height = 300;
            lbEntries.Name = airport.Profile.IATACode;
            lbEntries.ItemContainerStyleSelector = new ListBoxItemStyleSelector();

         

            destPanel.Children.Add(lbEntries);

         

            
            return destPanel;

        }
        //shows the entries
        private void showEntries()
        {
            showEntries(this.Route.Destination1);
            showEntries(this.Route.Destination2);
        }
        private void showEntries(Airport airport)
        {
            
            ListBox lb = (ListBox)LogicalTreeHelper.FindLogicalNode((Panel)this.Content, airport.Profile.IATACode);
            lb.Items.Clear();

            List<RouteTimeTableEntry> entries = this.Route.TimeTable.getEntries(airport);
            entries.Sort((delegate(RouteTimeTableEntry e1, RouteTimeTableEntry e2) { return e2.CompareTo(e1); }));

            foreach (RouteTimeTableEntry entry in entries)
                lb.Items.Add(entry);

           
        }
        //creates the buttons panel for cpu time table
        private WrapPanel createCPUButtonsPanel()
        {
            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

            Button btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnCPUOk_Click);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            buttonsPanel.Children.Add(btnOk);

            return buttonsPanel;
        }
      
      
      
        private void btnCPUOk_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

      
        //creates a header
        private TextBlock createHeader(Airport airport)
        {
            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtHeader.TextAlignment = TextAlignment.Left;
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format("To {0} ({1})",airport.Profile.Name,new AirportCodeConverter().Convert(airport));
            
            return txtHeader;
        }

    }
}
