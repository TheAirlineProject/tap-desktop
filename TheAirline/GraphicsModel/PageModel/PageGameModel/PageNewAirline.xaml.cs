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
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.GraphicsModel.PageModel.PageGameModel
{
    /// <summary>
    /// Interaction logic for PageNewAirline.xaml
    /// </summary>
  
    public partial class PageNewAirline : StandardPage
    {
        private TextBox txtAirlineName, txtIATA;
        private ComboBox cbCountry, cbColor;
        private Image imgLogo;
        private Button btnCreate;
        private string logoPath = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\default.png";
        private Route.RouteType airlinerType;
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

            // chs, 2011-20-10 changed to enable loading of logo

            WrapPanel panelAirlinerType = new WrapPanel();

            RadioButton rbPassenger = new RadioButton();
            rbPassenger.Content = "Passenger";
            rbPassenger.GroupName = "AirlineFocus";
            rbPassenger.IsChecked = true;
            rbPassenger.Tag = Route.RouteType.Passenger;
            rbPassenger.Checked += rbAirlineType_Checked;
            rbPassenger.Margin = new Thickness(5, 0, 0, 0);

            panelAirlinerType.Children.Add(rbPassenger);

            RadioButton rbCargo = new RadioButton();
            rbCargo.Content = "Cargo";
            rbCargo.GroupName = "AirlineFocus";
            rbCargo.Tag = Route.RouteType.Cargo;
            rbCargo.Checked += rbAirlineType_Checked;

            panelAirlinerType.Children.Add(rbCargo);

            lbContent.Items.Add(new QuickInfoValue("Airline type", panelAirlinerType));

            WrapPanel panelAirlineLogo = new WrapPanel();
      
            imgLogo = new Image();
            imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));
            imgLogo.Width = 32;
            imgLogo.Margin = new Thickness(0, 0, 5, 0);
            imgLogo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RenderOptions.SetBitmapScalingMode(imgLogo, BitmapScalingMode.HighQuality);

            panelAirlineLogo.Children.Add(imgLogo);

            Button btnLogo = new Button();
            btnLogo.Content = "...";
            btnLogo.Background = Brushes.Transparent;
            btnLogo.Click += new RoutedEventHandler(btnLogo_Click);
            btnLogo.Margin = new Thickness(5, 0, 0, 0);

            panelAirlineLogo.Children.Add(btnLogo);
           
            lbContent.Items.Add(new QuickInfoValue("Airline Logo", panelAirlineLogo));

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

            cbCountry.SelectedItem = Countries.GetCountry("122");

            lbContent.Items.Add(new QuickInfoValue("Country", cbCountry));

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);
            panelContent.Children.Add(panelButtons);

            btnCreate = new Button();
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Click += new RoutedEventHandler(btnCreate_Click);
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = "Create Airline";
            btnCreate.IsEnabled = false;
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            panelButtons.Children.Add(btnCreate);

            Button btnCancel = new Button();
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = "Cancel";
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            panelButtons.Children.Add(btnCancel);

            base.setTopMenu(new PageTopMenu());

            base.hideNavigator();

            base.hideBottomMenu();

            base.setContent(panelContent);

            base.setHeaderContent("Create New Airline");

            showPage(this);

            airlinerType = Route.RouteType.Passenger;

        }

        private void rbAirlineType_Checked(object sender, RoutedEventArgs e)
        {
            airlinerType = (Route.RouteType)((RadioButton)sender).Tag;
        }
        // chs, 2011-20-10 changed to enable loading of logo

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".png";
            dlg.Filter = "Images (.png)|*.png";
            dlg.InitialDirectory = AppSettings.getDataPath() + "\\graphics\\airlinelogos\\";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {

                   logoPath = dlg.FileName;
                   imgLogo.Source = new BitmapImage(new Uri(logoPath, UriKind.RelativeOrAbsolute));
                   
                

            }

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
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2401"), string.Format(Translator.GetInstance().GetString("MessageBox", "2401", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    createAirline();
                }
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2402"), Translator.GetInstance().GetString("MessageBox", "2402", "message"), WPFMessageBoxButtons.YesNo);

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

            AirlineProfile profile = new AirlineProfile(name, iata, color,"Unknown",false,1950,2199);
            profile.Countries = new List<Country>() { country };
            profile.Country = country;
            profile.addLogo(new AirlineLogo(logoPath));
            
            Airline airline = new Airline(profile,Airline.AirlineMentality.Aggressive,Airline.AirlineFocus.Local, Airline.AirlineLicense.Domestic,airlinerType);

            Airlines.AddAirline(airline);

           
            PageNavigator.NavigateTo(new PageNewGame());
        }
    }
}
