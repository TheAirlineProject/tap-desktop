using System;
using System.Collections.Generic;
using System.Globalization;
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
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpRouteFacilities.xaml
    /// </summary>
    public partial class PopUpRouteFacilities : PopUpWindow
    {
        private RouteAirlinerClass AirlinerClass;
        private ComboBox cbFood, cbDrinks, cbCrew, cbSeating;
        private TextBox txtPrice;
        private Button btnOk;
   
        public static object ShowPopUp(RouteAirlinerClass aClass)
        {
            PopUpWindow window = new PopUpRouteFacilities(aClass);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpRouteFacilities(RouteAirlinerClass aClass)
        {
            this.Uid = "1000";
            this.AirlinerClass = new RouteAirlinerClass(aClass.Type,aClass.Seating, aClass.FarePrice);
            this.AirlinerClass.CabinCrew = aClass.CabinCrew;
            this.AirlinerClass.DrinksFacility = aClass.DrinksFacility;
            this.AirlinerClass.FoodFacility = aClass.FoodFacility;
        
            InitializeComponent();

            this.Title = Translator.GetInstance().GetString("PopUpRouteFacilities", this.Uid);

            this.Width = 400;

            this.Height = 200;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel contentPanel = new StackPanel();

         

            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            contentPanel.Children.Add(lbRouteInfo);

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

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpRouteFacilities", "1001"), cbFood));

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

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpRouteFacilities", "1002"), cbDrinks));

            cbCrew = new ComboBox();
            cbCrew.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbCrew.Background = Brushes.Transparent;
            cbCrew.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCrew.Width = 150;

            for (int i = 1; i < 10; i++)
                cbCrew.Items.Add(i);

            cbCrew.SelectedIndex = 0;

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpRouteFacilities", "1003"), cbCrew));

            // chs, 2011-18-10 added for type of seating
            cbSeating = new ComboBox();
            cbSeating.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbSeating.Width = 150;
            cbSeating.ItemTemplate = this.Resources["SeatingItem"] as DataTemplate;

            foreach (RouteAirlinerClass.SeatingType sType in Enum.GetValues(typeof(RouteAirlinerClass.SeatingType)))
                cbSeating.Items.Add(sType);

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpRouteFacilities", "1004"), cbSeating));
                
            WrapPanel panelPrice = new WrapPanel();

            txtPrice = new TextBox();
            txtPrice.Background = Brushes.Transparent;
            txtPrice.Width = 100;
            txtPrice.TextAlignment = TextAlignment.Right;
            txtPrice.Margin = new Thickness(2, 0, 0, 0);
            txtPrice.PreviewKeyDown += new KeyEventHandler(txtPrice_PreviewKeyDown);
            txtPrice.PreviewTextInput += new TextCompositionEventHandler(txtPrice_PreviewTextInput);
            txtPrice.TextChanged += new TextChangedEventHandler(txtPrice_TextChanged);
          

            panelPrice.Children.Add(txtPrice);

            CultureInfo cultureInfo = new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, false);


            TextBlock txtCurrencySign = UICreator.CreateTextBlock(cultureInfo.NumberFormat.CurrencySymbol);
            txtCurrencySign.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            panelPrice.Children.Add(txtCurrencySign);

            lbRouteInfo.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpRouteFacilities", "1005"), panelPrice));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            contentPanel.Children.Add(panelButtons);

            btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height =Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            //btnOk.Margin = new System.Windows.Thickness(0, 5, 0, 0);
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.IsEnabled = false;
          
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            this.Content = contentPanel;

            cbFood.SelectedItem = this.AirlinerClass.FoodFacility;
            cbDrinks.SelectedItem = this.AirlinerClass.DrinksFacility;// RouteFacilities.GetBasicFacility(RouteFacility.FacilityType.Drinks);
            cbCrew.SelectedItem = this.AirlinerClass.CabinCrew;
            txtPrice.Text = this.AirlinerClass.FarePrice.ToString();
            cbSeating.SelectedItem = this.AirlinerClass.Seating;
        }

        private void txtPrice_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if (txtPrice.Text.Length == 1 && (e.Key == Key.Delete || e.Key == Key.Back))
                e.Handled = true;
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOk.IsEnabled = Convert.ToDouble(txtPrice.Text) < 10000 && Convert.ToDouble(txtPrice.Text)>0 ? true : false; 
    
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            double price = Convert.ToDouble(txtPrice.Text);
            RouteFacility food = (RouteFacility)cbFood.SelectedItem;
            RouteFacility drinks = (RouteFacility)cbDrinks.SelectedItem;
            int crew = (int)cbCrew.SelectedItem;
            RouteAirlinerClass.SeatingType seating = (RouteAirlinerClass.SeatingType)cbSeating.SelectedItem;

            this.AirlinerClass.Seating = seating;
            this.AirlinerClass.FarePrice = price;
            this.AirlinerClass.FoodFacility = food;
            this.AirlinerClass.DrinksFacility = drinks;
            this.AirlinerClass.CabinCrew = crew;

            this.Selected = this.AirlinerClass;
            this.Close();
        }
        private void txtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double number;
            Boolean parseable = double.TryParse(e.Text, out number);

            int length = txtPrice.Text.Length;

            e.Handled = !parseable || (length == 0 && number == 0);

          }
        private void cbFacility_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDrinks.SelectedItem != null && cbFood.SelectedItem != null)
            {
                int minimumValue = Math.Max(((RouteFacility)cbFood.SelectedItem).MinimumCabinCrew, ((RouteFacility)cbDrinks.SelectedItem).MinimumCabinCrew);

                int selectedValue = (int)cbCrew.SelectedItem;
                cbCrew.Items.Clear();

                for (int i = minimumValue; i < 10; i++)
                    cbCrew.Items.Add(i);

                cbCrew.SelectedItem = Math.Max(selectedValue, minimumValue);

                //createAirlinersList();
            }

        }

    }
}
