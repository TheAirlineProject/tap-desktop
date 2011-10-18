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
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;

namespace TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel
{
    public class PanelNewRoute : StackPanel
    {
        private TextBlock txtDistance, txtNoAssignments, txtFlightCode;
        private ComboBox cbDestination1, cbDestination2, cbFlightCode, cbAirliner;
        private Button btnSave;
        private PageRoutes ParentPage;
        private double MaxDistance;
        private Dictionary<AirlinerClass.ClassType, RouteAirlinerClass> Classes;
        public PanelNewRoute(PageRoutes parent)
        {
            this.Classes = new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>();

            var query = from a in AirlinerTypes.GetTypes().FindAll((delegate(AirlinerType t) { return t.Produced.From < GameObject.GetInstance().GameTime.Year; }))
                        select a.Range;

            this.MaxDistance = query.Max();

            this.ParentPage = parent;

            this.Margin = new Thickness(0, 0, 50, 0);
            //this.Margin = new Thickness(0, 10, 50, 0);
            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Create New Route";
            this.Children.Add(txtHeader);



            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.Children.Add(lbRouteInfo);

            // lbRouteInfo.Items.Add(new QuickInfoValue("Price", UICreator.CreateTextBlock(string.Format("${0:0,0}", this.Airliner.Price))));
            // lbRouteInfo.Items.Add(new QuickInfoValue("Yearly maintenance", UICreator.CreateTextBlock(string.Format("${0:0,0}", this.Airliner.Type.getMaintenance()))));


            cbDestination1 = createDestinationComboBox(); ;


            //cbDestination1.SelectedIndex = 0;

            lbRouteInfo.Items.Add(new QuickInfoValue("Destination 1", cbDestination1));


            cbDestination2 = createDestinationComboBox();

            lbRouteInfo.Items.Add(new QuickInfoValue("Destination 2", cbDestination2));


            txtDistance = UICreator.CreateTextBlock("-");
            lbRouteInfo.Items.Add(new QuickInfoValue("Distance", txtDistance));
            lbRouteInfo.Items.Add(new QuickInfoValue("Max. Distance", UICreator.CreateTextBlock(string.Format("{0:0.00} {1}", new NumberToUnitConverter().Convert(this.MaxDistance), new StringToLanguageConverter().Convert("km.")))));

            /*
            cbFood = new ComboBox();
            cbFood.Background = Brushes.Transparent;
            cbFood.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFood.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbFood.DisplayMemberPath = "Name";
            cbFood.SelectedValuePath = "Name";
            cbFood.SelectionChanged += new SelectionChangedEventHandler(cbFacility_SelectionChanged);
            cbFood.Width = 150;

            foreach (RouteFacility facility in RouteFacilities.GetFacilities(RouteFacility.FacilityType.Food))
                cbFood.Items.Add(facility);

            lbRouteInfo.Items.Add(new QuickInfoValue("Food on board", cbFood));

            cbDrinks = new ComboBox();
            cbDrinks.Background = Brushes.Transparent;
            cbDrinks.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDrinks.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbDrinks.DisplayMemberPath = "Name";
            cbDrinks.SelectedValuePath = "Name";
            cbDrinks.SelectionChanged += new SelectionChangedEventHandler(cbFacility_SelectionChanged);
            cbDrinks.Width = 150;

            foreach (RouteFacility facility in RouteFacilities.GetFacilities(RouteFacility.FacilityType.Drinks))
                cbDrinks.Items.Add(facility);

            lbRouteInfo.Items.Add(new QuickInfoValue("Drinks on board", cbDrinks));

            cbCrew = new ComboBox();
            cbCrew.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbCrew.Background = Brushes.Transparent;
            cbCrew.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCrew.Width = 150;

            for (int i = 1; i < 10; i++)
                cbCrew.Items.Add(i);

            cbCrew.SelectedIndex = 0;

            lbRouteInfo.Items.Add(new QuickInfoValue("Cabin crew on board", cbCrew));

            WrapPanel panelPrice = new WrapPanel();

            txtPrice = new TextBox();
            txtPrice.Background = Brushes.Transparent;
            txtPrice.Width = 100;
            txtPrice.TextAlignment = TextAlignment.Right;
            txtPrice.Margin = new Thickness(2, 0, 0, 0);
            txtPrice.PreviewTextInput += new TextCompositionEventHandler(txtPrice_PreviewTextInput);
            txtPrice.Text = "10";

            panelPrice.Children.Add(txtPrice);
         
            CultureInfo cultureInfo = new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo, false);
         

            TextBlock txtCurrencySign = UICreator.CreateTextBlock(cultureInfo.NumberFormat.CurrencySymbol);
            txtCurrencySign.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            panelPrice.Children.Add(txtCurrencySign);

            lbRouteInfo.Items.Add(new QuickInfoValue("Fare price", panelPrice));
            */

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                RouteAirlinerClass rClass = new RouteAirlinerClass(type,RouteAirlinerClass.SeatingType.Reserved_Seating, 10);
                rClass.FoodFacility = RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Food);
                rClass.DrinksFacility = RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
                this.Classes.Add(type, rClass);

                WrapPanel panelClassButtons = new WrapPanel();

                Button btnEdit = new Button();
                btnEdit.Background = Brushes.Transparent;
                btnEdit.Tag = type;
                btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                //btnEdit.Height = 16;
                //btnEdit.Width = 16;

                Image imgEdit = new Image();
                imgEdit.Width = 16;
                imgEdit.Source = new BitmapImage(new Uri(@"/Data/images/edit.png", UriKind.RelativeOrAbsolute));
                RenderOptions.SetBitmapScalingMode(imgEdit, BitmapScalingMode.HighQuality);

                btnEdit.Content = imgEdit;

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


                ContentControl lblClass= new ContentControl();
                lblClass.SetResourceReference(ContentControl.ContentTemplateProperty, "RouteAirlinerClassItem");
                lblClass.Content = rClass;

                brdToolTip.Child = lblClass;


                imgInfo.ToolTip = brdToolTip;

                panelClassButtons.Children.Add(imgInfo);

       

                lbRouteInfo.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type, null, null, null).ToString(), panelClassButtons));
            }

            WrapPanel panelAssigned = new WrapPanel();

            cbAirliner = new ComboBox();
            //cbAirliner.Background = Brushes.Transparent;
            cbAirliner.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirliner.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirliner.DisplayMemberPath = "Name";
            cbAirliner.SelectedValuePath = "Name";
            cbAirliner.Width = 200;
            cbAirliner.Visibility = System.Windows.Visibility.Collapsed;

            panelAssigned.Children.Add(cbAirliner);


            txtNoAssignments = UICreator.CreateTextBlock("No airliner to assign");
            panelAssigned.Children.Add(txtNoAssignments);


            lbRouteInfo.Items.Add(new QuickInfoValue("Assign airliner", panelAssigned));

            txtFlightCode = new TextBlock();

            cbFlightCode = new ComboBox();
            cbFlightCode.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbFlightCode.Width = 100;
            cbFlightCode.SelectionChanged += new SelectionChangedEventHandler(cbFlightCode_SelectionChanged);

            for (int i = 0; i < GameObject.GetInstance().HumanAirline.getFlightCodes().Count-2; i += 2)
                cbFlightCode.Items.Add(GameObject.GetInstance().HumanAirline.getFlightCodes()[i]);

            cbFlightCode.SelectedIndex = 0;

            lbRouteInfo.Items.Add(new QuickInfoValue("Homebound flight code", cbFlightCode));
            lbRouteInfo.Items.Add(new QuickInfoValue("Outbound flight code", txtFlightCode));

            btnSave = new Button();
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = 20;
            btnSave.Width = 80;
            btnSave.Content = "Save";
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnSave.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnSave.IsEnabled = false;
            this.Children.Add(btnSave);




        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            AirlinerClass.ClassType type = (AirlinerClass.ClassType)((Button)sender).Tag;
            RouteAirlinerClass aClass = (RouteAirlinerClass)PopUpRouteFacilities.ShowPopUp(this.Classes[type]);

            if (aClass != null)
            {
                this.Classes[type].CabinCrew = aClass.CabinCrew;
                this.Classes[type].DrinksFacility = aClass.DrinksFacility;
                this.Classes[type].FarePrice = aClass.FarePrice;
                this.Classes[type].FoodFacility = aClass.FoodFacility;
                this.Classes[type].Seating = aClass.Seating;
            }
        }

        private void cbFlightCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbFlightCode.SelectedIndex * 2 + 1;

            txtFlightCode.Text = GameObject.GetInstance().HumanAirline.getFlightCodes()[index];
        }
        //creates the list of possible airliners
        private void createAirlinersList()
        {
            Airport airport1 = (Airport)cbDestination1.SelectedItem;
            Airport airport2 = (Airport)cbDestination2.SelectedItem;

            if (airport1 != null && airport2 != null)
            {
                double distance = MathHelpers.GetDistance(airport1.Profile.Coordinates, airport2.Profile.Coordinates);

                int minCrews = getMinCrews();//Math.Max(((RouteFacility)cbFood.SelectedItem).MinimumCabinCrew, ((RouteFacility)cbDrinks.SelectedItem).MinimumCabinCrew);

                cbAirliner.Items.Clear();

                foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
                    if ((!airliner.HasRoute && airliner.Airliner.Type.Range > distance && airliner.Airliner.Type.CabinCrew >= minCrews) && !airliner.HasRoute)
                        cbAirliner.Items.Add(airliner);

                cbAirliner.Visibility = cbAirliner.Items.Count == 0 ? Visibility.Collapsed : System.Windows.Visibility.Visible;
                txtNoAssignments.Visibility = cbAirliner.Visibility == System.Windows.Visibility.Collapsed ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            }

        }
        //returns the min crews
        private int getMinCrews()
        {
            int minCrew = int.MaxValue;

            foreach (RouteAirlinerClass aClass in this.Classes.Values)
            {
                if (minCrew > aClass.CabinCrew)
                    minCrew = aClass.CabinCrew;
            }
            return minCrew;
        }
        //returns if the two destinations are in the correct area (the airport types are ok)
        private Boolean isRouteInCorrectArea()
        {
            Airport dest1 = (Airport)cbDestination1.SelectedItem;
            Airport dest2 = (Airport)cbDestination2.SelectedItem;



            return (dest1.Profile.Country == dest2.Profile.Country || (dest1.Profile.Country.Region == dest2.Profile.Country.Region && (dest1.Profile.Type == AirportProfile.AirportType.Regional || dest1.Profile.Type == AirportProfile.AirportType.International) && (dest2.Profile.Type == AirportProfile.AirportType.Regional || dest2.Profile.Type == AirportProfile.AirportType.International)) || (dest1.Profile.Type == AirportProfile.AirportType.International && dest2.Profile.Type == AirportProfile.AirportType.International));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Airline airline = GameObject.GetInstance().HumanAirline;
            //double price;
            //Boolean parseable = double.TryParse(txtPrice.Text, out price);

            Airport dest1 = (Airport)cbDestination1.SelectedItem;
            Airport dest2 = (Airport)cbDestination2.SelectedItem;

            // if (parseable && price > 0 && price < 10000 && dest1.Gates.getFreeGates(airline) > 0 && dest2.Gates.getFreeGates(airline) > 0)
            if (dest1.Gates.getFreeGates(airline) > 0 && dest2.Gates.getFreeGates(airline) > 0)
            {


                Guid id = Guid.NewGuid();
                Route route = new Route(id.ToString(),dest1, dest2, 0, cbFlightCode.SelectedItem.ToString(), txtFlightCode.Text);

                foreach (RouteAirlinerClass aClass in this.Classes.Values)
                {
                    route.getRouteAirlinerClass(aClass.Type).CabinCrew = aClass.CabinCrew;
                    route.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                    route.getRouteAirlinerClass(aClass.Type).FoodFacility = aClass.FoodFacility;
                    route.getRouteAirlinerClass(aClass.Type).DrinksFacility = aClass.DrinksFacility;
                    route.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;
                    // route.DrinksFacility = (RouteFacility)cbDrinks.SelectedItem;
                    // route.FoodFacility = (RouteFacility)cbFood.SelectedItem;
                    // route.CabinCrew = (int)cbCrew.SelectedItem;
                }

                airline.addRoute(route);

                dest1.Gates.getEmptyGate(airline).Route = route;
                dest2.Gates.getEmptyGate(airline).Route = route;

                this.ParentPage.showRoutes();

                this.Visibility = System.Windows.Visibility.Collapsed;

                if (cbAirliner.SelectedItem != null)
                {
                    FleetAirliner airliner = (FleetAirliner)cbAirliner.SelectedItem;
                    RouteAirliner rAirliner = new RouteAirliner(airliner, route);

                    airliner.RouteAirliner = rAirliner;
                }
            }
            else
                WPFMessageBox.Show("Invalid route", "The route is not valid which might be because of no free gates at the destinations or that the price is not correct.", WPFMessageBoxButtons.Ok);

        }

        //creates the combo box for a destination
        private ComboBox createDestinationComboBox()
        {
            ComboBox cbDestination = new ComboBox();


            cbDestination.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            //cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbDestination.Background = Brushes.Transparent;
            cbDestination.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDestination.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbDestination.SelectionChanged += new SelectionChangedEventHandler(cbDestination_SelectionChanged);
            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports;
            airports.Sort(delegate(Airport a1, Airport a2) { return a1.Profile.Name.CompareTo(a2.Profile.Name); });

            foreach (Airport airport in airports)
                cbDestination.Items.Add(airport);

            return cbDestination;


        }

        private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDestination2.SelectedItem != null && cbDestination1.SelectedItem != null)
            {
                Airport airport1 = (Airport)cbDestination1.SelectedItem;
                Airport airport2 = (Airport)cbDestination2.SelectedItem;



                double distance = MathHelpers.GetDistance(airport1.Profile.Coordinates, airport2.Profile.Coordinates);
                txtDistance.Text = string.Format("{0:0.00} {1}", new NumberToUnitConverter().Convert(distance), new StringToLanguageConverter().Convert("km."));

                btnSave.IsEnabled = distance > 50 && distance < this.MaxDistance && isRouteInCorrectArea();

                createAirlinersList();
            }
        }
    }
}
