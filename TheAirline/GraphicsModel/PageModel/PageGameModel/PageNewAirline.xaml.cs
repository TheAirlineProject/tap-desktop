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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using System.IO;
using System.Reflection;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageNewAirline.xaml
    /// </summary>
    // chs, 2011-19-10 added for the possibility of creating a new airline
  
    public partial class PageNewAirline : StandardPage
    {
        private TextBox txtAirlineName, txtIATA;
        private ComboBox cbCountry, cbColor;
        private Button btnCreate;
        private string defaultLogoPath = Setup.getDataPath() + "\\graphics\\airlinelogos\\default.png";
        public PageNewAirline()
        {
            InitializeComponent();

            StackPanel panelContent = new StackPanel();
            panelContent.Margin = new Thickness(10, 0, 10, 0);
            panelContent.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            Panel panelLogo = UICreator.CreateGameLogo();
            panelLogo.Margin = new Thickness(0, 0, 0, 20);

            panelContent.Children.Add(panelLogo);

            TextBlock txtHeader = new TextBlock();
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = "Airline Profile";
            panelContent.Children.Add(txtHeader);

            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelContent.Children.Add(lbContent);

            txtAirlineName = new TextBox();
            txtAirlineName.Background = Brushes.Transparent;
            txtAirlineName.BorderBrush = Brushes.Black;
            txtAirlineName.Width = 200;
            txtAirlineName.TextChanged+=new TextChangedEventHandler(txtIATA_TextChanged);

            lbContent.Items.Add(new QuickInfoValue("Airline Name", txtAirlineName));

            txtIATA = new TextBox();
            txtIATA.Background = Brushes.Transparent;
            txtIATA.BorderBrush = Brushes.Black;
            txtIATA.MaxLength = 2;
            txtIATA.Width = 25;
            txtIATA.TextChanged += new TextChangedEventHandler(txtIATA_TextChanged);

            lbContent.Items.Add(new QuickInfoValue("IATA Code", txtIATA));

           
            Image imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(defaultLogoPath, UriKind.RelativeOrAbsolute));
            imgLogo.Width = 32;
            imgLogo.Margin = new Thickness(0, 0, 5, 0);
            imgLogo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);
          
           
            lbContent.Items.Add(new QuickInfoValue("Airline Logo", imgLogo));

            cbColor = new ComboBox();
            cbColor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbColor.Width = 250;
            cbColor.ItemTemplate = this.Resources["ColorItem"] as DataTemplate;

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
                 cbColor.Items.Add(c);

            cbColor.SelectedIndex = 0;
   
            lbContent.Items.Add(new QuickInfoValue("Airline Color", cbColor));

            cbCountry = new ComboBox();
            cbCountry.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbCountry.SetResourceReference(ComboBox.ItemTemplateProperty, "CountryFlagLongItem");
            cbCountry.Width = 250;

            List<Country> countries = Countries.GetCountries();
            countries.Sort(delegate(Country c1, Country c2) { return c1.Name.CompareTo(c2.Name); });

            foreach (Country country in countries)
                cbCountry.Items.Add(country);

            cbCountry.SelectedItem = Countries.GetCountry("United States");

            lbContent.Items.Add(new QuickInfoValue("Country", cbCountry));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelContent.Children.Add(panelButtons);

            btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = 20;
            btnCreate.Width = 100;
            btnCreate.Content = "Create Airline";
            btnCreate.IsEnabled = false;
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCreate);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = 20;
            btnCancel.Width = 100;
            btnCancel.Content = "Cancel";
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            panelButtons.Children.Add(btnCancel);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Create New Game");

            showPage(this);

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            PageNavigator.NavigateTo(new PageNewGame());
        }

        private void txtIATA_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnCreate.IsEnabled = txtIATA.Text.Trim().Length == 2 && txtAirlineName.Text.Trim().Length > 2;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = Airlines.GetAirline(txtIATA.Text.Trim().ToUpper());

            if (airline != null)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("IATA exits", string.Format("Are you sure you want to override {0}?", airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    createAirline();
                }
            }
            else
            {
                
                WPFMessageBoxResult result = WPFMessageBox.Show("Create airline", "Are you sure you want to create this airline?", WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    createAirline();
                }
            }
        }
        //creates the airline
        private void createAirline()
        {
            Airline tAirline = Airlines.GetAirline(txtIATA.Text.Trim().ToUpper());

            if (tAirline != null)
                Airlines.RemoveAirline(tAirline);

            string name = txtAirlineName.Text.Trim();
            string iata = txtIATA.Text.Trim().ToUpper();
            Country country = (Country)cbCountry.SelectedItem;
            string color = ((PropertyInfo)cbColor.SelectedItem).Name;

            AirlineProfile profile = new AirlineProfile(name, iata, color, country, "Unknown");
            profile.Logo = defaultLogoPath;

            Airline airline = new Airline(profile);

            Airlines.AddAirline(airline);

            //Recreate Airports + Load/save Airline Profile

            PageNavigator.NavigateTo(new PageNewGame());
        }
    }
}
