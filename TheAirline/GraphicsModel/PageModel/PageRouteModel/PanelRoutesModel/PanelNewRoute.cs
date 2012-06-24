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
using TheAirline.Model.PassengerModel;

namespace TheAirline.GraphicsModel.PageModel.PageRouteModel.PanelRoutesModel
{
    public class PanelNewRoute : StackPanel
    {
        private TextBlock txtDistance,  txtFlightCode, txtInvalidRoute,txtFlightRestrictions, txtDestination1Gates, txtDestination2Gates;
        private ComboBox cbDestination1, cbDestination2;
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
         
            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PanelNewRoute", "201");
            this.Children.Add(txtHeader);



            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            this.Children.Add(lbRouteInfo);

            WrapPanel panelDestination1 = new WrapPanel();

            cbDestination1 = createDestinationComboBox(); 
            panelDestination1.Children.Add(cbDestination1);

            txtDestination1Gates = new TextBlock();
            txtDestination1Gates.Margin = new Thickness(5, 0, 0, 0);
            txtDestination1Gates.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelDestination1.Children.Add(txtDestination1Gates);


            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewRoute", "202"), panelDestination1));

            WrapPanel panelDestination2 = new WrapPanel();
                        
            cbDestination2 = createDestinationComboBox();
            panelDestination2.Children.Add(cbDestination2);

            txtDestination2Gates = new TextBlock();
            txtDestination2Gates.Margin = new Thickness(5, 0, 0, 0);
            txtDestination2Gates.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            panelDestination2.Children.Add(txtDestination2Gates);

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewRoute", "203"), panelDestination2));


            txtDistance = UICreator.CreateTextBlock("-");
            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewRoute", "204"), txtDistance));
            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PanelNewRoute", "205"), UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(this.MaxDistance), new StringToLanguageConverter().Convert("km.")))));

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

            txtFlightCode = new TextBlock();

            btnSave = new Button();
            btnSave.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnSave.Height = Double.NaN;
            btnSave.Width = Double.NaN;
            btnSave.Content = "Save";
            btnSave.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnSave.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnSave.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnSave.IsEnabled = false;
            this.Children.Add(btnSave);

            txtInvalidRoute = UICreator.CreateTextBlock(Translator.GetInstance().GetString("PanelNewRoute","1001"));
            txtInvalidRoute.Foreground = Brushes.DarkRed;
            txtInvalidRoute.Visibility = System.Windows.Visibility.Collapsed;
            this.Children.Add(txtInvalidRoute);

            txtFlightRestrictions = UICreator.CreateTextBlock("");
            txtFlightRestrictions.Foreground = Brushes.DarkRed;
            txtFlightRestrictions.Visibility = System.Windows.Visibility.Collapsed;
            this.Children.Add(txtFlightRestrictions);

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

            double distance = MathHelpers.GetDistance(dest1.Profile.Coordinates, dest2.Profile.Coordinates);

            return (dest1.Profile.Country == dest2.Profile.Country || distance<1000 ||(dest1.Profile.Country.Region == dest2.Profile.Country.Region && (dest1.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International) && (dest2.Profile.Type == AirportProfile.AirportType.Short_Haul_International || dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International)) || (dest1.Profile.Type == AirportProfile.AirportType.Long_Haul_International && dest2.Profile.Type == AirportProfile.AirportType.Long_Haul_International));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            Airline airline = GameObject.GetInstance().HumanAirline;
            Airport dest1 = (Airport)cbDestination1.SelectedItem;
            Airport dest2 = (Airport)cbDestination2.SelectedItem;

            if (dest1.Terminals.getFreeGates(airline) > 0 && dest2.Terminals.getFreeGates(airline) > 0)
            {


                Guid id = Guid.NewGuid();
                Route route = new Route(id.ToString(),dest1, dest2, 0);

                foreach (RouteAirlinerClass aClass in this.Classes.Values)
                {
                    route.getRouteAirlinerClass(aClass.Type).CabinCrew = aClass.CabinCrew;
                    route.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                    route.getRouteAirlinerClass(aClass.Type).FoodFacility = aClass.FoodFacility;
                    route.getRouteAirlinerClass(aClass.Type).DrinksFacility = aClass.DrinksFacility;
                    route.getRouteAirlinerClass(aClass.Type).Seating = aClass.Seating;
        
                }
               
                airline.addRoute(route);

                dest1.Terminals.getEmptyGate(airline).Route = route;
                dest2.Terminals.getEmptyGate(airline).Route = route;

                this.ParentPage.showRoutes();

                this.Visibility = System.Windows.Visibility.Collapsed;

                route.LastUpdated = GameObject.GetInstance().GameTime; 
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2501"), Translator.GetInstance().GetString("MessageBox", "2501", "message"), WPFMessageBoxButtons.Ok);

        }

        //creates the combo box for a destination
        private ComboBox createDestinationComboBox()
        {
            ComboBox cbDestination = new ComboBox();


            cbDestination.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbDestination.Background = Brushes.Transparent;
            cbDestination.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbDestination.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbDestination.SelectionChanged += new SelectionChangedEventHandler(cbDestination_SelectionChanged);
            List<Airport> airports = GameObject.GetInstance().HumanAirline.Airports.FindAll(a=>a.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline)>0);
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

                foreach (RouteAirlinerClass aClass in this.Classes.Values)
                {
                    aClass.FarePrice = PassengerHelpers.GetPassengerPrice(airport1, airport2) * GeneralHelpers.ClassToPriceFactor(aClass.Type);
                }

                double distance = MathHelpers.GetDistance(airport1.Profile.Coordinates, airport2.Profile.Coordinates);
                txtDistance.Text = string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(distance), new StringToLanguageConverter().Convert("km."));

                btnSave.IsEnabled = distance > 50 && distance < this.MaxDistance && isRouteInCorrectArea() && !FlightRestrictions.HasRestriction(airport1.Profile.Country,airport2.Profile.Country,GameObject.GetInstance().GameTime,FlightRestriction.RestrictionType.Flights) &&  !FlightRestrictions.HasRestriction(airport2.Profile.Country,airport1.Profile.Country,GameObject.GetInstance().GameTime,FlightRestriction.RestrictionType.Flights) && !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline,airport1.Profile.Country,airport2.Profile.Country,GameObject.GetInstance().GameTime);
                
                txtInvalidRoute.Visibility = isRouteInCorrectArea() ? Visibility.Collapsed : Visibility.Visible;
                txtFlightRestrictions.Visibility =FlightRestrictions.HasRestriction(airport2.Profile.Country,airport1.Profile.Country,GameObject.GetInstance().GameTime,FlightRestriction.RestrictionType.Flights) ||  !FlightRestrictions.HasRestriction(GameObject.GetInstance().HumanAirline,airport1.Profile.Country,airport2.Profile.Country,GameObject.GetInstance().GameTime) || FlightRestrictions.HasRestriction(airport1.Profile.Country,airport2.Profile.Country,GameObject.GetInstance().GameTime,FlightRestriction.RestrictionType.Flights) ? Visibility.Visible : System.Windows.Visibility.Collapsed;

                txtFlightRestrictions.Text= string.Format(Translator.GetInstance().GetString("PanelNewRoute","1002"),airport1.Profile.Country.Name,airport2.Profile.Country.Name);

            }
            
            Airport airport = (Airport)((ComboBox)sender).SelectedItem;

            TextBlock txtDestinationGates = cbDestination2 == ((ComboBox)sender) ? txtDestination2Gates : txtDestination1Gates;

            txtDestinationGates.Text = string.Format(Translator.GetInstance().GetString("PanelNewRoute", "206"), airport.Terminals.getFreeGates(GameObject.GetInstance().HumanAirline));
        }
    }
}
