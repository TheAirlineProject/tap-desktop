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
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.Model.AirlineModel;
using System.Reflection;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.Converters;
using TheAirline.Model.AirlinerModel.RouteModel;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpNewSubsidiary.xaml
    /// </summary>
    public partial class PopUpNewSubsidiary : PopUpWindow
    {
        private TextBox txtIATA, txtAirlineName;
        private Slider slMoney;
        private Button btnOk;
        private Image imgLogo;
        private ComboBox cbColor, cbAirport;
        private string logoPath;
        private Route.RouteType airlineType;
        public static object ShowPopUp()
        {
            PopUpWindow window = new PopUpNewSubsidiary();
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpNewSubsidiary()
        {
            
            InitializeComponent();

            logoPath = GameObject.GetInstance().HumanAirline.Profile.Logo;

            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpNewSubsidiary", this.Uid);

            this.Width = 500;

            this.Height = 400;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);
            
            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            mainPanel.Children.Add(lbContent);

            txtAirlineName = new TextBox();
            txtAirlineName.Background = Brushes.Transparent;
            txtAirlineName.BorderBrush = Brushes.Black;
            txtAirlineName.Width = 200;
            txtAirlineName.TextChanged += new TextChangedEventHandler(txtIATA_TextChanged);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1001"), txtAirlineName));

            txtIATA = new TextBox();
            txtIATA.Background = Brushes.Transparent;
            txtIATA.BorderBrush = Brushes.Black;
            txtIATA.MaxLength = 2;
            txtIATA.Width = 25;
            txtIATA.TextChanged += new TextChangedEventHandler(txtIATA_TextChanged);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1002"), txtIATA));

            WrapPanel panelAirlinerType = new WrapPanel();

            RadioButton rbPassenger = new RadioButton();
            rbPassenger.IsChecked = true;
            rbPassenger.Content = Translator.GetInstance().GetString("PopUpNewSubsidiary", "1007");
            rbPassenger.Margin = new Thickness(5, 0, 0, 0);
            rbPassenger.Checked += rbAirlineType_Checked;
            rbPassenger.Tag = Route.RouteType.Passenger;
            rbPassenger.GroupName = "AirlineType";

            panelAirlinerType.Children.Add(rbPassenger);

            RadioButton rbCargo = new RadioButton();
            rbCargo.Tag = Route.RouteType.Cargo;
            rbCargo.GroupName = "AirlineType";
            rbCargo.Content = Translator.GetInstance().GetString("PopUpNewSubsidiary", "1008");
            rbCargo.Tag = Route.RouteType.Cargo;
            rbCargo.Checked+=rbAirlineType_Checked;

            panelAirlinerType.Children.Add(rbCargo);

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1009"), panelAirlinerType));
            
            airlineType = Route.RouteType.Passenger;

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

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1003"), panelAirlineLogo));

            cbColor = new ComboBox();
            cbColor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbColor.Width = 250;
            cbColor.ItemTemplate = this.Resources["ColorItem"] as DataTemplate;

            foreach (PropertyInfo c in typeof(Colors).GetProperties())
            {
                cbColor.Items.Add(c);

                if (c.Name == GameObject.GetInstance().MainAirline.Profile.Color)
                    cbColor.SelectedItem = c;
            }

        
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1004"), cbColor));

            cbAirport = new ComboBox();
            cbAirport.SetResourceReference(ComboBox.ItemTemplateProperty, "AirportCountryItem");
            cbAirport.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");

            foreach (Airport airport in GameObject.GetInstance().MainAirline.Airports.FindAll(a=>a.Terminals.getFreeGates()>0))
            {
                cbAirport.Items.Add(airport);
            }

            cbAirport.SelectedIndex = 0;

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary", "1005"), cbAirport));

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpNewSubsidiary","1006"),createMoneySlider()));
            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
        }

        private void rbAirlineType_Checked(object sender, RoutedEventArgs e)
        {
            airlineType = (Route.RouteType)((RadioButton)sender).Tag;
        }
        //creates the buttons panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 10, 0, 0);

            btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
            btnOk.IsEnabled = false;
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCancel);

            return panelButtons;
        }
        //creates the slider for the money
        private WrapPanel createMoneySlider()
        {
            double minValue = 100000;
            double maxValue = GameObject.GetInstance().MainAirline.Money / 2;
            WrapPanel sliderPanel = new WrapPanel();

            TextBlock txtValue = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(minValue).ToString());//UICreator.CreateTextBlock(string.Format("{0:C}", minValue));
            txtValue.VerticalAlignment = VerticalAlignment.Bottom;
            txtValue.Margin = new Thickness(5, 0, 0, 0);
        
            slMoney = new Slider();
            slMoney.Width = 200;
            slMoney.Value = minValue;
            slMoney.Tag = txtValue;
            slMoney.Maximum = maxValue;
            slMoney.Minimum = minValue;
            slMoney.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slMoney.TickFrequency = (maxValue - minValue) / slMoney.Width;
            slMoney.IsSnapToTickEnabled = true;
            slMoney.IsMoveToPointEnabled = true;
            sliderPanel.Children.Add(slMoney);

            sliderPanel.Children.Add(txtValue);

            return sliderPanel;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextBlock)slider.Tag;
            txtBlock.Text = new ValueCurrencyConverter().Convert(slider.Value).ToString();//string.Format("{0:C}", slider.Value);

        }
        private void txtIATA_TextChanged(object sender, TextChangedEventArgs e)
        {
            Boolean IATAExits = Airlines.GetAllAirlines().Find(a=>a.Profile.IATACode == txtIATA.Text.ToUpper()) != null;
            Boolean airportSelected = cbAirport.SelectedItem != null;

            btnOk.IsEnabled = txtIATA.Text.Trim().Length == 2 && txtAirlineName.Text.Trim().Length > 2 && !IATAExits && airportSelected;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)cbAirport.SelectedItem;
            string color = ((PropertyInfo)cbColor.SelectedItem).Name;
                  
            AirlineProfile profile = new AirlineProfile(txtAirlineName.Text.Trim(),txtIATA.Text.ToUpper().Trim(),color,GameObject.GetInstance().MainAirline.Profile.CEO,false,GameObject.GetInstance().GameTime.Year,2199);
            profile.addLogo(new AirlineLogo(logoPath));
            profile.Country = GameObject.GetInstance().MainAirline.Profile.Country;
            
            SubsidiaryAirline subAirline = new SubsidiaryAirline(GameObject.GetInstance().MainAirline,profile,Airline.AirlineMentality.Safe,Airline.AirlineFocus.Local,Airline.AirlineLicense.Domestic,airlineType);
            subAirline.addAirport(airport);
            subAirline.Money = slMoney.Value;

            this.Selected = subAirline;
            this.Close();

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }

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

    }
}
