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
        private ComboBox cbAirliner;
        private ListBox lbRouteFinances;
        private Dictionary<AirlinerClass.ClassType, RouteAirlinerClass> Classes;
        public PanelRoute(PageRoutes parent, Route route)
        {
            this.Classes = new Dictionary<AirlinerClass.ClassType, RouteAirlinerClass>();

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
            lbRouteInfo.Items.Add(new QuickInfoValue("Distance", UICreator.CreateTextBlock(string.Format("{0:0} {1}", new NumberToUnitConverter().Convert(distance),new StringToLanguageConverter().Convert("km.")))));

            foreach (AirlinerClass.ClassType type in Enum.GetValues(typeof(AirlinerClass.ClassType)))
            {
                RouteAirlinerClass rClass = this.Route.getRouteAirlinerClass(type);
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
                
                btnEdit.Visibility = this.Route.Airliner != null && this.Route.Airliner.Status != RouteAirliner.AirlinerStatus.Stopped ? Visibility.Collapsed : System.Windows.Visibility.Visible;
           
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

            WrapPanel panelAssigned = new WrapPanel();

            cbAirliner = new ComboBox();
            cbAirliner.Background = Brushes.Transparent;
            cbAirliner.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirliner.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirliner.DisplayMemberPath = "Name";
            cbAirliner.SelectedValuePath = "Name";
            cbAirliner.Width = 200;

            panelAssigned.Children.Add(cbAirliner);

            int minCrews = getMinCrews();//Math.Max(this.Route.FoodFacility.MinimumCabinCrew, this.Route.DrinksFacility.MinimumCabinCrew);

            foreach (FleetAirliner airliner in GameObject.GetInstance().HumanAirline.Fleet)
                if (airliner.Airliner.Type.MinRunwaylength <= this.Route.Destination1.getMaxRunwayLength() && airliner.Airliner.Type.MinRunwaylength <= this.Route.Destination2.getMaxRunwayLength() &&(!airliner.HasRoute && airliner.Airliner.Type.Range > distance && airliner.Airliner.Type.CabinCrew >= minCrews) || (airliner.HasRoute && (this.Route.Airliner != null && this.Route.Airliner.Airliner == airliner)))
                    cbAirliner.Items.Add(airliner);

            if (this.Route.Airliner != null)
                cbAirliner.SelectedItem = this.Route.Airliner.Airliner;
            
            TextBlock txtAssigned = UICreator.CreateTextBlock(this.Route.Airliner == null ? "No airliner to assign" : this.Route.Airliner.Airliner.Name);
            panelAssigned.Children.Add(txtAssigned);

            cbAirliner.Visibility = this.Route.Airliner != null && this.Route.Airliner.Status != RouteAirliner.AirlinerStatus.Stopped || cbAirliner.Items.Count==0 ? Visibility.Collapsed : System.Windows.Visibility.Visible;
            txtAssigned.Visibility = cbAirliner.Visibility == System.Windows.Visibility.Collapsed ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            
            lbRouteInfo.Items.Add(new QuickInfoValue("Assigned airliner", panelAssigned));

            lbRouteInfo.Items.Add(new QuickInfoValue("Homebound flight code",UICreator.CreateTextBlock(this.Route.TimeTable.getRouteEntryDestinations()[1].FlightCode)));
            lbRouteInfo.Items.Add(new QuickInfoValue("Outbound flight code", UICreator.CreateTextBlock(this.Route.TimeTable.getRouteEntryDestinations()[0].FlightCode)));


            WrapPanel buttonsPanel = new WrapPanel();
            buttonsPanel.Margin = new Thickness(0, 5, 0, 0);

            this.Children.Add(buttonsPanel);

            buttonsPanel.Visibility = this.Route.Airliner != null && this.Route.Airliner.Status != RouteAirliner.AirlinerStatus.Stopped ? Visibility.Collapsed : System.Windows.Visibility.Visible;

            Button btnOk = new Button();
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = "OK";
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            // btnDelete.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            buttonsPanel.Children.Add(btnOk);

            Button btnRemove = new Button();
            btnRemove.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnRemove.Width = Double.NaN;
            btnRemove.Height = Double.NaN;
            btnRemove.Content = "Remove Airliner";
            btnRemove.Click += new RoutedEventHandler(btnRemove_Click);
            btnRemove.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnRemove.Visibility = this.Route.Airliner != null ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            buttonsPanel.Children.Add(btnRemove);

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

            Button btnTimeTable = new Button();
            btnTimeTable.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTimeTable.Height = Double.NaN;
            btnTimeTable.Width = Double.NaN;
            btnTimeTable.Content = "Timetable";
            btnTimeTable.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTimeTable.Margin = new Thickness(5, 0, 0, 0);
            btnTimeTable.Click += new RoutedEventHandler(btnTimeTable_Click);

            this.Children.Add(createRouteFinancesPanel());

            showRouteFinances();

            GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PanelRoute_OnTimeChanged);
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


            lbRouteFinances= new ListBox();
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
                 lbRouteFinances.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type,null,null,null).ToString(),UICreator.CreateTextBlock(string.Format("{0:C}", this.Route.getRouteInvoiceAmount(type)))));

            
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
        private void btnTimeTable_Click(object sender, RoutedEventArgs e)
        {
            RouteTimeTable timeTable = (RouteTimeTable)PopUpTimeTable.ShowPopUp(GameObject.GetInstance().HumanAirline, this.Route);

            if (timeTable != null)
            {
                this.Route.TimeTable = timeTable;
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2502"), string.Format(Translator.GetInstance().GetString("MessageBox", "2502", "message"), this.Route.Airliner.Airliner.Name), WPFMessageBoxButtons.YesNo);

             if (result == WPFMessageBoxResult.Yes)
             {

                 this.Route.Airliner.Airliner.RouteAirliner = null;

                 this.Route.Airliner = null;

                 this.Route.Invoices.Clear();
                 this.Route.Statistics.clear();

                 this.Visibility = System.Windows.Visibility.Collapsed;
             }


        }
       
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            //double price;
            //Boolean parseable = double.TryParse(txtPrice.Text, out price);

            if (!(cbAirliner.SelectedItem == null || (this.Route.Airliner != null && ((FleetAirliner)cbAirliner.SelectedItem) == this.Route.Airliner.Airliner)))
            {
                FleetAirliner airliner = (FleetAirliner)cbAirliner.SelectedItem;

                if (this.Route.Airliner != null)
                {
                    this.Route.Airliner.Airliner.RouteAirliner = null;
                }

                RouteAirliner rAirliner = new RouteAirliner(airliner, this.Route);

                airliner.RouteAirliner = rAirliner;

                this.Route.Invoices.Clear();
                this.Route.Statistics.clear();

            }
       
                foreach (RouteAirlinerClass aClass in this.Classes.Values)
                {
                    this.Route.getRouteAirlinerClass(aClass.Type).CabinCrew = aClass.CabinCrew;
                    this.Route.getRouteAirlinerClass(aClass.Type).FarePrice = aClass.FarePrice;
                    this.Route.getRouteAirlinerClass(aClass.Type).FoodFacility = aClass.FoodFacility;
                    this.Route.getRouteAirlinerClass(aClass.Type).DrinksFacility = aClass.DrinksFacility;

                 
                }
            

            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2503"), string.Format(Translator.GetInstance().GetString("MessageBox", "2503", "message"), this.Route.Destination1.Profile.Name, this.Route.Destination2.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                GameObject.GetInstance().HumanAirline.removeRoute(this.Route);

                if (this.Route.Airliner != null)
                    this.Route.Airliner.Airliner.RouteAirliner = null;

                this.Route.Destination1.Terminals.getUsedGate(GameObject.GetInstance().HumanAirline).Route = null;
                this.Route.Destination2.Terminals.getUsedGate(GameObject.GetInstance().HumanAirline).Route = null;

                this.ParentPage.showRoutes();

                this.Visibility = System.Windows.Visibility.Collapsed;



            }


        }
    }
}
