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
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpRouteFacilities.xaml
    /// </summary>
    public partial class PopUpRouteFacilities : PopUpWindow
    {
        private RouteAirlinerClass AirlinerClass;
        private ComboBox cbSeating;
        private TextBox txtPrice;
        private Button btnOk;
        private List<ComboBox> cbFacilities;

        public static object ShowPopUp(RouteAirlinerClass aClass)
        {
            PopUpWindow window = new PopUpRouteFacilities(aClass);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpRouteFacilities(RouteAirlinerClass aClass)
        {
            this.cbFacilities = new List<ComboBox>();

            this.Uid = "1000";
            this.AirlinerClass = new RouteAirlinerClass(aClass.Type,aClass.Seating, aClass.FarePrice);
         
            foreach (RouteFacility facility in aClass.getFacilities())
                this.AirlinerClass.addFacility(facility);
          
            InitializeComponent();

            this.Title = Translator.GetInstance().GetString("PopUpRouteFacilities", this.Uid);

            this.Width = 400;

            this.Height = 250;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel contentPanel = new StackPanel();

            ListBox lbRouteInfo = new ListBox();
            lbRouteInfo.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbRouteInfo.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            contentPanel.Children.Add(lbRouteInfo);

            foreach (RouteFacility.FacilityType type in Enum.GetValues(typeof(RouteFacility.FacilityType)))
            {
                if (GameObject.GetInstance().GameTime.Year >= (int)type)
                {
                    ComboBox cbFacility = new ComboBox();
                    cbFacility.Background = Brushes.Transparent;
                    cbFacility.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
                    cbFacility.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    cbFacility.DisplayMemberPath = "Name";
                    cbFacility.SelectedValuePath = "Name";
                    cbFacility.Tag = type;
                    cbFacility.Width = 150;

                    foreach (RouteFacility facility in AirlineHelpers.GetRouteFacilities(GameObject.GetInstance().HumanAirline,type))
                        cbFacility.Items.Add(facility);

                    cbFacilities.Add(cbFacility);

                    lbRouteInfo.Items.Add(new QuickInfoValue(new TextUnderscoreConverter().Convert(type).ToString(), cbFacility));

                    cbFacility.SelectedItem = this.AirlinerClass.getFacility(type);
                }
            }
            
          
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
            CountryCurrency currency = null;
            
            if (GameObject.GetInstance().CurrencyCountry != null)
                currency = GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);


            TextBlock txtCurrencySign = UICreator.CreateTextBlock(currency == null ? cultureInfo.NumberFormat.CurrencySymbol : currency.CurrencySymbol);
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

            double fareprice = currency == null ? this.AirlinerClass.FarePrice : this.AirlinerClass.FarePrice * currency.Rate;
            
             txtPrice.Text = String.Format("{0:0.##}",fareprice);
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
            btnOk.IsEnabled = Convert.ToDouble(txtPrice.Text) < 100000 && Convert.ToDouble(txtPrice.Text)>0 ? true : false; 
    
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            double price = Convert.ToDouble(txtPrice.Text);
            RouteAirlinerClass.SeatingType seating = (RouteAirlinerClass.SeatingType)cbSeating.SelectedItem;

            this.AirlinerClass.Seating = seating;
            this.AirlinerClass.FarePrice = price;
         
            foreach (ComboBox cbFacility in cbFacilities)
                this.AirlinerClass.addFacility((RouteFacility)cbFacility.SelectedItem);

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
       

    }
}
