using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel
{
    public class PanelRoute : StackPanel
    {

        private Route Route;
        private PageRoutes ParentPage;
        private ListBox lbRouteFinances;
        private Dictionary<Route, Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>> Classes;
        public PanelRoute(PageRoutes parent, Route route)
        {

            this.Classes = new Dictionary<Route, Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>>();

            this.ParentPage = parent;

            this.Route = route;

            this.Margin = new Thickness(0, 0, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Route Information";
            this.Children.Add(txtHeader);

            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.Children.Add(lbRouteInfo);

            double distance = MathHelpers.GetDistance(this.Route.Destination1.Profile.Coordinates, this.Route.Destination2.Profile.Coordinates);

            lbRouteInfo.Items.Add(new QuickInfoValue("Destination 1", UICreator.CreateTextBlock(this.Route.Destination1.Profile.Name)));
            lbRouteInfo.Items.Add(new QuickInfoValue("Destination 2", UICreator.CreateTextBlock(this.Route.Destination2.Profile.Name)));

            if (this.Route.getStopovers().Count > 0)
                lbRouteInfo.Items.Add(new QuickInfoValue("Stops", UICreator.CreateTextBlock(string.Join(", ", from s in this.Route.getStopovers() select new AirportCodeConverter().Convert(s.Stopover).ToString()))));

            lbRouteInfo.Items.Add(new QuickInfoValue("Total distance", UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(distance), new StringToLanguageConverter().Convert("km.")))));

            this.Classes.Add(route, new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>());

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                RouteAirlinerClass rClass = this.Route.getRouteAirlinerClass(type);
                this.Classes[route].Add(type, rClass);

                WrapPanel panelClassButtons = new WrapPanel();

                Button btnEdit = new Button();
                btnEdit.Background = Brushes.Transparent;
                btnEdit.Tag = new KeyValuePair<Route, AirlinerClass.ClassType>(this.Route, type);
                btnEdit.Click += new RoutedEventHandler(btnEdit_Click);


                Image imgEdit = new Image();
                imgEdit.Width = 16;
                imgEdit.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
                RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);

                btnEdit.Content = imgEdit;

                btnEdit.Visibility = this.Route.HasAirliner && (this.Route.getCurrentAirliner() != null && this.Route.getCurrentAirliner().Status != FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Collapsed : System.Windows.Visibility.Visible;

                panelClassButtons.Children.Add(btnEdit);

                Image imgInfo = new Image();
                imgInfo.Width = 16;
                imgInfo.Source = new BitmapImage(new Uri(@"/Data/images/info.png", UriKind.RelativeOrAbsolute));
                imgInfo.Margin = new Thickness(5, 0, 0, 0);
                imgInfo.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                RenderOptions.SetBitmapScalingMode(imgInfo, BitmapScalingMode.HighQuality);

                Border brdToolTip = new Border();
                brdToolTip.Margin = new Thickness(-4, 0, -4, -3);
                brdToolTip.Padding = new Thickness(5);
                brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");
                
                ContentControl lblClass = new ContentControl();
                lblClass.SetResourceReference(ContentControl.ContentTemplateProperty, "RouteAirlinerClassItem");
                lblClass.Content = rClass;

                brdToolTip.Child = lblClass;


                imgInfo.ToolTip = brdToolTip;

                panelClassButtons.Children.Add(imgInfo);



                lbRouteInfo.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type, null, null, null).ToString(), panelClassButtons));
            }

            foreach (StopoverRoute stopover in this.Route.getStopovers())
            {
                foreach (Route leg in stopover.Legs)
                {
                    this.Classes.Add(leg, new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>());
                    lbRouteInfo.Items.Add(new QuickInfoValue("Stopover", UICreator.CreateTextBlock(string.Format("{0}-{1}", new AirportCodeConverter().Convert(leg.Destination1), new AirportCodeConverter().Convert(leg.Destination2)))));
                    foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                    {
                        RouteAirlinerClass rClass = leg.getRouteAirlinerClass(type);
                        this.Classes[leg].Add(type, rClass);

                        WrapPanel panelClassButtons = new WrapPanel();

                        Button btnEdit = new Button();
                        btnEdit.Background = Brushes.Transparent;
                        btnEdit.Tag = new KeyValuePair<Route, AirlinerClass.ClassType>(leg, type);
                        btnEdit.Click += new RoutedEventHandler(btnEdit_Click);

                        Image imgEdit = new Image();
                        imgEdit.Width = 16;
                        imgEdit.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
                        RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);

                        btnEdit.Content = imgEdit;

                        btnEdit.Visibility = this.Route.HasAirliner && (this.Route.getCurrentAirliner() != null && this.Route.getCurrentAirliner().Status != FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Collapsed : System.Windows.Visibility.Visible;

                        panelClassButtons.Children.Add(btnEdit);

                        Image imgInfo = new Image();
                        imgInfo.Width = 16;
                        imgInfo.Source = new BitmapImage(new Uri(@"/Data/images/info.png", UriKind.RelativeOrAbsolute));
                        imgInfo.Margin = new Thickness(5, 0, 0, 0);
                        imgInfo.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                        RenderOptions.SetBitmapScalingMode(imgInfo, BitmapScalingMode.HighQuality);

                        Border brdToolTip = new Border();
                        brdToolTip.Margin = new Thickness(-4, 0, -4, -3);
                        brdToolTip.Padding = new Thickness(5);
                        brdToolTip.SetResourceReference(Border.BackgroundProperty, "HeaderBackgroundBrush2");

                        ContentControl lblClass = new ContentControl();
                        lblClass.SetResourceReference(ContentControl.ContentTemplateProperty, "RouteAirlinerClassItem");
                        lblClass.Content = rClass;

                        brdToolTip.Child = lblClass;


                        imgInfo.ToolTip = brdToolTip;

                        panelClassButtons.Children.Add(imgInfo);

                        lbRouteInfo.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type).ToString(), panelClassButtons));
                    }
                }
            }

            int numberOfInboundFlights = this.Route.TimeTable.Entries.Count(e => e.DepartureAirport == this.Route.Destination2);
            int numberOfOutboundFlights = this.Route.TimeTable.Entries.Count(e => e.DepartureAirport == this.Route.Destination1);

            lbRouteInfo.Items.Add(new QuickInfoValue("Flights per week (out/in)", UICreator.CreateTextBlock(string.Format("{0} / {1}", numberOfOutboundFlights, numberOfInboundFlights))));

            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            this.Children.Add(buttonsPanel);

            buttonsPanel.Visibility = this.Route.HasAirliner && (this.Route.getCurrentAirliner() != null && this.Route.getCurrentAirliner().Status != FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Collapsed : System.Windows.Visibility.Visible;

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            buttonsPanel.Children.Add(btnOk);

            Button btnLoad = new Button();
            btnLoad.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoad.Height = Double.NaN;
            btnLoad.Width = Double.NaN;
            btnLoad.Content = Translator.GetInstance().GetString("General", "115");
            btnLoad.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
            btnLoad.Margin = new Thickness(5, 0, 0, 0);
            buttonsPanel.Children.Add(btnLoad);

            Button btnDelete = new Button();
            btnDelete.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDelete.Height = Double.NaN;
            btnDelete.Width = Double.NaN;
            btnDelete.Content = "Delete Route";
            btnDelete.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDelete.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            btnDelete.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            buttonsPanel.Children.Add(btnDelete);


            this.Children.Add(createRouteFinancesPanel());

            showRouteFinances();

        }

        private void PanelRoute_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PanelRoute_OnTimeChanged);

        }

        private void PanelRoute_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                showRouteFinances();
            }
        }
        //creates the finances for the route
        private StackPanel createRouteFinancesPanel()
        {
            StackPanel panelRouteFinances = new StackPanel();
            panelRouteFinances.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Route Finances";
            panelRouteFinances.Children.Add(txtHeader);


            lbRouteFinances = new ListBox();
            lbRouteFinances.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteFinances.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelRouteFinances.Children.Add(lbRouteFinances);

            return panelRouteFinances;
        }
        //shows the finances for the route
        private void showRouteFinances()
        {
            lbRouteFinances.Items.Clear();

            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
                lbRouteFinances.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type, null, null, null).ToString(), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Route.getRouteInvoiceAmount(type)).ToString())));//string.Format("{0:C}", this.Route.getRouteInvoiceAmount(type)))));


        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<Route, AirlinerClass.ClassType> type = (KeyValuePair<Route, AirlinerClass.ClassType>)((Button)sender).Tag;

            RouteAirlinerClass aClass = (RouteAirlinerClass)PopUpRouteFacilities.ShowPopUp(this.Classes[type.Key][type.Value]);

            if (aClass != null)
            {

                foreach (RouteFacility facility in aClass.getFacilities())
                    this.Classes[type.Key][type.Value].addFacility(facility);
                
                this.Classes[type.Key][type.Value].FarePrice = aClass.FarePrice;
                this.Classes[type.Key][type.Value].Seating = aClass.Seating;


            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            ComboBox cbConfigurations = new ComboBox();
            cbConfigurations.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbConfigurations.SelectedValuePath = "Name";
            cbConfigurations.DisplayMemberPath = "Name";
            cbConfigurations.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbConfigurations.Width = 200;

            foreach (RouteClassesConfiguration confItem in Configurations.GetConfigurations(Configuration.ConfigurationType.Routeclasses))
                cbConfigurations.Items.Add(confItem);

            cbConfigurations.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp("Select configuration", cbConfigurations) == PopUpSingleElement.ButtonSelected.OK && cbConfigurations.SelectedItem != null)
            {
                RouteClassesConfiguration configuration = (RouteClassesConfiguration)cbConfigurations.SelectedItem;

                foreach (RouteClassConfiguration classConfiguration in configuration.getClasses())
                {
                    foreach (Route route in this.Classes.Keys)
                        foreach (RouteFacility facility in classConfiguration.getFacilities())
                            this.Classes[route][classConfiguration.Type].addFacility(facility);

                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {

            foreach (Route route in this.Classes.Keys)
                foreach (RouteAirlinerClass aClass in this.Classes[route].Values)
                {
                    route.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;

                    List<RouteFacility> facilities = new List<RouteFacility>(aClass.getFacilities());
                    foreach (RouteFacility facility in facilities)
                        route.getRouteAirlinerClass(aClass.Type).addFacility(facility);


                }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2503"), string.Format(Translator.GetInstance().GetString("MessageBox", "2503", "message"), this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.removeRoute(this.Route);

                if (this.Route.HasAirliner)
                    this.Route.getAirliners().ForEach(a => a.removeRoute(this.Route));

                this.Route.Destination1.Terminals.getUsedGate(GameObject.GetInstance().HumanAirline).HasRoute = false;
                this.Route.Destination2.Terminals.getUsedGate(GameObject.GetInstance().HumanAirline).HasRoute = false;

                this.ParentPage.showRoutes();

                this.Visibility = System.Windows.Visibility.Collapsed;



            }


        }
    }
}
