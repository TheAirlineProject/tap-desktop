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
using TheAirline.GraphicsModel.UserControlModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel
{
    public class PanelRoute : StackPanel
    {
        private Frame frmStopoverStatistics;
        private Route Route;
        private PageRoutes ParentPage;
        private ListBox lbRouteFinances;
        private Dictionary<Route, Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>> Classes;
        private double CargoPrice;
        private TextBlock txtCargo;
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
            txtHeader.Uid = "1000";
            txtHeader.Text = Translator.GetInstance().GetString("PanelRoute", txtHeader.Uid);
            this.Children.Add(txtHeader);

            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.Children.Add(lbRouteInfo);

            double distance = MathHelpers.GetDistance(this.Route.Destination1.Profile.Coordinates, this.Route.Destination2.Profile.Coordinates);

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute","1016"),UICreator.CreateTextBlock(this.Route.Type.ToString())));
            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute","1001"), UICreator.CreateTextBlock(this.Route.Destination1.Profile.Name)));
            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute","1002"), UICreator.CreateTextBlock(this.Route.Destination2.Profile.Name)));

            if (this.Route.HasStopovers)
                lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1003"), UICreator.CreateTextBlock(string.Join(", ", from s in this.Route.Stopovers select new AirportCodeConverter().Convert(s.Stopover).ToString()))));

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1004"), UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(distance), new StringToLanguageConverter().Convert("km.")))));

            if (this.Route.Type == Route.RouteType.Passenger || this.Route.Type == Route.RouteType.Mixed)
            {
                this.Classes.Add(route, new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>());

                foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                {
                    RouteAirlinerClass rClass = ((PassengerRoute)this.Route).getRouteAirlinerClass(type);
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

                    Boolean inRoute = route.getAirliners().Exists(a => a.Status != FleetAirliner.AirlinerStatus.Stopped);
                    //btnEdit.Visibility = this.Route.HasAirliner && (this.Route.getCurrentAirliner() != null && this.Route.getCurrentAirliner().Status != FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Collapsed : System.Windows.Visibility.Visible;
                    btnEdit.Visibility = inRoute ? Visibility.Collapsed : System.Windows.Visibility.Visible;

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
            }
            if (this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Mixed || this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Cargo)
            {
                this.CargoPrice = ((CargoRoute)this.Route).PricePerUnit;

                WrapPanel panelCargo = new WrapPanel();

                txtCargo = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.CargoPrice).ToString());
                txtCargo.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                panelCargo.Children.Add(txtCargo);

                Button btnEditCargo = new Button();
                btnEditCargo.Margin = new Thickness(5, 0, 0, 0);
                btnEditCargo.Background = Brushes.Transparent;
                btnEditCargo.Click += new RoutedEventHandler(btnEditCargo_Click);

                Image imgEdit = new Image();
                imgEdit.Width = 16;
                imgEdit.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
                RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);

                btnEditCargo.Content = imgEdit;

                panelCargo.Children.Add(btnEditCargo);

                lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute","1015"),panelCargo));
            }
            foreach (StopoverRoute stopover in this.Route.Stopovers)
            {
                foreach (Route leg in stopover.Legs)
                {

                    lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1005"), UICreator.CreateTextBlock(string.Format("{0}-{1}", new AirportCodeConverter().Convert(leg.Destination1), new AirportCodeConverter().Convert(leg.Destination2)))));
                  
                    if (this.Route.Type == Route.RouteType.Passenger || this.Route.Type == Route.RouteType.Mixed)
                    {
                        this.Classes.Add(leg, new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>());
               
                        foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
                        {
                            RouteAirlinerClass rClass = ((PassengerRoute)leg).getRouteAirlinerClass(type);
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
            }

            int numberOfInboundFlights = this.Route.TimeTable.Entries.Count(e => e.DepartureAirport == this.Route.Destination2);
            int numberOfOutboundFlights = this.Route.TimeTable.Entries.Count(e => e.DepartureAirport == this.Route.Destination1);

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1006"), UICreator.CreateTextBlock(string.Format("{0} / {1}", numberOfOutboundFlights, numberOfInboundFlights))));

            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            this.Children.Add(buttonsPanel);

            buttonsPanel.Visibility = this.Route.HasAirliner && (this.Route.getCurrentAirliner() != null && this.Route.getCurrentAirliner().Status != FleetAirliner.AirlinerStatus.Stopped) ? Visibility.Collapsed : System.Windows.Visibility.Visible;

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Uid = "100";
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            buttonsPanel.Children.Add(btnOk);

            Button btnLoad = new Button();
            btnLoad.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoad.Height = Double.NaN;
            btnLoad.Width = Double.NaN;
            btnLoad.Uid = "115";
            btnLoad.Content = Translator.GetInstance().GetString("General", btnLoad.Uid);
            btnLoad.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
            btnLoad.Margin = new Thickness(5, 0, 0, 0);
            btnLoad.Visibility = this.Route.Type == Route.RouteType.Cargo ? Visibility.Collapsed : System.Windows.Visibility.Visible;
            buttonsPanel.Children.Add(btnLoad);

            Button btnDelete = new Button();
            btnDelete.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnDelete.Height = Double.NaN;
            btnDelete.Width = Double.NaN;
            btnDelete.Uid = "200";
            btnDelete.Content = Translator.GetInstance().GetString("PanelRoute", btnDelete.Uid);
            btnDelete.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnDelete.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            btnDelete.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            buttonsPanel.Children.Add(btnDelete);

            //if (this.Route.HasStopovers)
            {
                this.Children.Add(createStopoverStatisticsPanel());
            }
            //else
            {
              //  this.Children.Add(createRouteFinancesPanel());
            }

            showRouteFinances();

        }
        //creates the panel for stopover statistics
        private StackPanel createStopoverStatisticsPanel()
        {
            StackPanel panelStopoverStatistics = new StackPanel();

            WrapPanel panelMenuButtons = new WrapPanel();
            panelStopoverStatistics.Children.Add(panelMenuButtons);

            ucSelectButton sbRouteFinances = new ucSelectButton();
            sbRouteFinances.Uid = "1007";
            sbRouteFinances.Content = Translator.GetInstance().GetString("PanelRoute", sbRouteFinances.Uid);
            sbRouteFinances.Click += sbRouteFinances_Click;
            sbRouteFinances.IsSelected = true;
            panelMenuButtons.Children.Add(sbRouteFinances);

            if (this.Route.HasStopovers)
            {
                foreach (Route leg in this.Route.Stopovers.SelectMany(s => s.Legs))
                {
                    ucSelectButton sbLeg = new ucSelectButton();
                    sbLeg.Content = string.Format("{0}-{1}", new AirportCodeConverter().Convert(leg.Destination1), new AirportCodeConverter().Convert(leg.Destination2));
                    sbLeg.Tag = leg;
                    sbLeg.Click += sbLeg_Click;
                    panelMenuButtons.Children.Add(sbLeg);
                }
            }
            else
            {
                ucSelectButton sbStatistics = new ucSelectButton();
                sbStatistics.Content = string.Format("{0}-{1}", new AirportCodeConverter().Convert(this.Route.Destination1), new AirportCodeConverter().Convert(this.Route.Destination2));
                sbStatistics.Tag = this.Route;
                sbStatistics.Click += sbLeg_Click;
                panelMenuButtons.Children.Add(sbStatistics);
            }

            frmStopoverStatistics = new Frame();
            frmStopoverStatistics.Navigate(createRouteFinancesPanel());

            panelStopoverStatistics.Children.Add(frmStopoverStatistics);

            return panelStopoverStatistics;
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
            txtHeader.Uid = "1008";
            txtHeader.Text = Translator.GetInstance().GetString("PanelRoute", txtHeader.Uid);
            panelRouteFinances.Children.Add(txtHeader);

            lbRouteFinances = new ListBox();
            lbRouteFinances.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteFinances.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelRouteFinances.Children.Add(lbRouteFinances);

            return panelRouteFinances;
        }
        //creates the panel for the statistics for a leg
        private StackPanel createLegStatistics(Route leg)
        {
            StackPanel panelLegStatistics = new StackPanel();
            panelLegStatistics.Margin = new Thickness(0, 5, 0, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = string.Format("{0}-{1}", new AirportCodeConverter().Convert(leg.Destination1), new AirportCodeConverter().Convert(leg.Destination2));
            panelLegStatistics.Children.Add(txtHeader);
            
            ListBox lbLegStatistics = new ListBox();
            lbLegStatistics.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbLegStatistics.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            panelLegStatistics.Children.Add(lbLegStatistics);

            if (leg.Type == Route.RouteType.Mixed || leg.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Passenger)
            {
                RouteAirlinerClass raClass = ((PassengerRoute)leg).getRouteAirlinerClass(AirlinerClass.ClassType.Economy_Class);

                double passengers = leg.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers"));
                double avgPassengers = leg.Statistics.getStatisticsValue(raClass, StatisticsTypes.GetStatisticsType("Passengers%"));

                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1009"), UICreator.CreateTextBlock(String.Format("{0:0,0}", passengers))));
                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1010"), UICreator.CreateTextBlock(string.Format("{0:0.##}", avgPassengers))));
             }
            if (leg.Type == Route.RouteType.Mixed || leg.Type == Route.RouteType.Cargo)
            {
                double cargo = leg.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo"));
                double avgCargo = leg.Statistics.getStatisticsValue(StatisticsTypes.GetStatisticsType("Cargo%"));

                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1013"), UICreator.CreateTextBlock(String.Format("{0:0,0}", cargo))));
                lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1014"), UICreator.CreateTextBlock(string.Format("{0:0.##}", avgCargo))));
        
            }
            lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1011"), UICreator.CreateTextBlock(string.Format("{0:0.##} %", leg.FillingDegree * 100))));
     
            lbLegStatistics.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelRoute", "1012"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(leg.Balance).ToString())));

            return panelLegStatistics;
        }
        //shows the finances for the route
        private void showRouteFinances()
        {
            lbRouteFinances.Items.Clear();

            foreach (Invoice.InvoiceType type in this.Route.getRouteInvoiceTypes())
                lbRouteFinances.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type, null, null, null).ToString(), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Route.getRouteInvoiceAmount(type)).ToString())));//string.Format("{0:C}", this.Route.getRouteInvoiceAmount(type)))));


        }
        private void btnEditCargo_Click(object sender, RoutedEventArgs e)
        {
            double rate = 1;

            if (GameObject.GetInstance().CurrencyCountry != null)
            {
                CountryCurrency currency = GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);
                rate = currency == null ? 1 : currency.Rate;
            }

            TextBox tbCargoPrice = new TextBox();
            tbCargoPrice.Text = string.Format("{0:0.00}", this.CargoPrice * rate);
            tbCargoPrice.TextAlignment = TextAlignment.Left;
            tbCargoPrice.Width = 100;
            tbCargoPrice.Background = Brushes.Transparent;
            tbCargoPrice.SetResourceReference(TextBox.ForegroundProperty, "TextColor");
            tbCargoPrice.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelNewRoute", "1008"), tbCargoPrice) == PopUpSingleElement.ButtonSelected.OK && tbCargoPrice.Text.Length > 0)
            {
               
                this.CargoPrice = Convert.ToDouble(tbCargoPrice.Text) / rate;
                txtCargo.Text = new ValueCurrencyConverter().Convert(this.CargoPrice).ToString();
            }
        }
        private void sbLeg_Click(object sender, RoutedEventArgs e)
        {
            Route leg = (Route)((ucSelectButton)sender).Tag;

            frmStopoverStatistics.Navigate(createLegStatistics(leg));
        }

        private void sbRouteFinances_Click(object sender, RoutedEventArgs e)
        {
            frmStopoverStatistics.Navigate(createRouteFinancesPanel());

            showRouteFinances();
        }
      
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<Route, AirlinerClass.ClassType> type = (KeyValuePair<Route, AirlinerClass.ClassType>)((Button)sender).Tag;

            RouteAirlinerClass aClass = (RouteAirlinerClass)PopUpRouteFacilities.ShowPopUp(this.Classes[type.Key][type.Value]);

            if (aClass != null)
            {

                foreach (RouteFacility facility in aClass.getFacilities())
                    this.Classes[type.Key][type.Value].addFacility(facility);

                double rate = 1;

                if (GameObject.GetInstance().CurrencyCountry != null)
                {
                    CountryCurrency currency = GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);
                    rate = currency == null ? 1 : currency.Rate;
                }

                this.Classes[type.Key][type.Value].FarePrice = aClass.FarePrice / rate;
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
            if (this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Mixed || this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Passenger)
            {
                foreach (Route route in this.Classes.Keys)
                    foreach (RouteAirlinerClass aClass in this.Classes[route].Values)
                    {
                        ((PassengerRoute)route).getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;

                        List<RouteFacility> facilities = new List<RouteFacility>(aClass.getFacilities());
                        foreach (RouteFacility facility in facilities)
                            ((PassengerRoute)route).getRouteAirlinerClass(aClass.Type).addFacility(facility);


                    }
            }
            if (this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Cargo || this.Route.Type == Model.AirlinerModel.RouteModel.Route.RouteType.Mixed)
            {
                ((CargoRoute)this.Route).PricePerUnit = this.CargoPrice;
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


                PageNavigator.NavigateTo(new PageRoutes());

                this.Visibility = System.Windows.Visibility.Collapsed;



            }


        }
    }
}
