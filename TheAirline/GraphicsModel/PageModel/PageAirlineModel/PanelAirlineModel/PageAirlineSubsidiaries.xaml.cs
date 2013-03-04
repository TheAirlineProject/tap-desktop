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
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineSubsidiaries.xaml
    /// </summary>
    public partial class PageAirlineSubsidiaries : Page
    {
        private Airline Airline;
        private ListBox lbSubsidiaryAirline;
        private StandardPage PageParent;
        private ComboBox cbAirlineFrom, cbAirlineTo;
        private Slider slMoney;
        private StackPanel panelTransferFunds;
        public PageAirlineSubsidiaries(Airline airline, StandardPage parent)
        {
            this.PageParent = parent;
            this.Airline = airline;

            InitializeComponent();

            StackPanel panelSubsidiaries = new StackPanel();
            panelSubsidiaries.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtSubsidiariesHeader = new TextBlock();
            txtSubsidiariesHeader.Uid = "1001";
            txtSubsidiariesHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtSubsidiariesHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtSubsidiariesHeader.FontWeight = FontWeights.Bold;
            txtSubsidiariesHeader.Text = Translator.GetInstance().GetString("PageAirlineSubsidiaries", txtSubsidiariesHeader.Uid);

            panelSubsidiaries.Children.Add(txtSubsidiariesHeader);

            lbSubsidiaryAirline = new ListBox();
            lbSubsidiaryAirline.ItemTemplate = this.Resources["SubsidiaryItem"] as DataTemplate;
            lbSubsidiaryAirline.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSubsidiaryAirline.MaxHeight = GraphicsHelpers.GetContentHeight() - 100;

            showSubsidiaries();

            panelSubsidiaries.Children.Add(lbSubsidiaryAirline);

            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Visibility = this.Airline.IsHuman && !(this.Airline is SubsidiaryAirline) && this.Airline.Money>100000 ? Visibility.Visible : Visibility.Collapsed;
            panelButtons.Margin = new Thickness(0,5,0,0);

            panelSubsidiaries.Children.Add(panelButtons);

            Button btnCreate = new Button();
            btnCreate.Uid = "200"; 
            btnCreate.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCreate.Height = Double.NaN;
            btnCreate.Width = Double.NaN;
            btnCreate.Content = Translator.GetInstance().GetString("PageAirlineSubsidiaries",btnCreate.Uid);
            btnCreate.Click+=new RoutedEventHandler(btnCreate_Click);
            btnCreate.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");

            panelButtons.Children.Add(btnCreate);

            if (this.Airline.IsHuman)
                panelSubsidiaries.Children.Add(createTransferFundsPanel());
           
            this.Content = panelSubsidiaries;

            

        }
        //creates the panel to transfer funds between airlines
        private StackPanel createTransferFundsPanel()
        {
            panelTransferFunds = new StackPanel();
            panelTransferFunds.Margin = new Thickness(0, 5, 0, 0);
            panelTransferFunds.Visibility = this.Airline.Subsidiaries.Count > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1002";
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineSubsidiaries", txtHeader.Uid);

            panelTransferFunds.Children.Add(txtHeader);

            WrapPanel panelTransferAirlines = new WrapPanel();
            panelTransferAirlines.Margin = new Thickness(0, 5, 0, 0);
            panelTransferFunds.Children.Add(panelTransferAirlines);

            cbAirlineFrom = new ComboBox();
            cbAirlineFrom.SelectionChanged += cbAirlineFrom_SelectionChanged;
            cbAirlineFrom.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirlineFrom.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbAirlineFrom.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirlineFrom.Width = 200;

            cbAirlineFrom.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbAirlineFrom.Items.Add(airline);
        
            panelTransferAirlines.Children.Add(cbAirlineFrom);

            panelTransferAirlines.Children.Add(UICreator.CreateTextBlock("->"));

            cbAirlineTo = new ComboBox();
            cbAirlineTo.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAirlineTo.SetResourceReference(ComboBox.ItemTemplateProperty, "AirlineLogoItem");
            cbAirlineTo.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAirlineTo.Width = 200;

            cbAirlineTo.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbAirlineTo.Items.Add(airline);

            cbAirlineTo.SelectedItem = GameObject.GetInstance().HumanAirline;

            panelTransferAirlines.Children.Add(cbAirlineTo);

            panelTransferFunds.Children.Add(createMoneySlider());
            
            Button btnTransferFunds = new Button();
            btnTransferFunds.Uid = "201";
            btnTransferFunds.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnTransferFunds.Height = Double.NaN;
            btnTransferFunds.Width = Double.NaN;
            btnTransferFunds.Content = Translator.GetInstance().GetString("PageAirlineSubsidiaries", btnTransferFunds.Uid);
            btnTransferFunds.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnTransferFunds.Margin = new Thickness(0, 5, 0, 0);
            btnTransferFunds.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnTransferFunds.Click += btnTransferFunds_Click;

            panelTransferFunds.Children.Add(btnTransferFunds);

            cbAirlineFrom.SelectedItem = GameObject.GetInstance().HumanAirline;

            return panelTransferFunds;

        }

        private void cbAirlineFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbAirlineFrom.SelectedItem != null)
            {
                slMoney.Maximum = ((Airline)cbAirlineFrom.SelectedItem).Money / 2;

                if (slMoney.Maximum <= slMoney.Minimum)
                    slMoney.Minimum = slMoney.Maximum / 2;
         
                if (slMoney.Value > slMoney.Maximum)
                    slMoney.Value = slMoney.Maximum;

      
             }
        }

        private void btnTransferFunds_Click(object sender, RoutedEventArgs e)
        {
            double funds = (double)slMoney.Value;

            Airline airlineFrom = (Airline)cbAirlineFrom.SelectedItem;
            Airline airlineTo = (Airline)cbAirlineTo.SelectedItem;

            if (airlineFrom == airlineTo || funds > airlineFrom.Money)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2120"), string.Format(Translator.GetInstance().GetString("MessageBox", "2120", "message"), airlineFrom.Profile.Name,airlineTo.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {
            
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2121"), string.Format(Translator.GetInstance().GetString("MessageBox", "2121", "message"), funds,airlineFrom.Profile.Name,airlineTo.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    airlineFrom.Money -= funds;
                    airlineTo.Money += funds;
                }
            }
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
        //shows the airline to transfer between
        private void showTransferAirlines()
        {
            cbAirlineTo.Items.Clear();
            cbAirlineTo.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbAirlineTo.Items.Add(airline);

            cbAirlineTo.SelectedItem = GameObject.GetInstance().HumanAirline;

            cbAirlineFrom.Items.Clear();
            cbAirlineFrom.Items.Add(GameObject.GetInstance().MainAirline);

            foreach (Airline airline in GameObject.GetInstance().MainAirline.Subsidiaries)
                cbAirlineFrom.Items.Add(airline);

            cbAirlineFrom.SelectedItem = GameObject.GetInstance().HumanAirline;


        }
        //shows the subsidiary airlines
        private void showSubsidiaries()
        {
            lbSubsidiaryAirline.Items.Clear();

            this.Airline.Subsidiaries.ForEach(s => lbSubsidiaryAirline.Items.Add(s));

            if (panelTransferFunds != null)
            {
                panelTransferFunds.Visibility = this.Airline.IsHuman && this.Airline.Subsidiaries.Count > 0 ? Visibility.Visible : System.Windows.Visibility.Collapsed;
                showTransferAirlines();
            }
        }
        
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)PopUpNewSubsidiary.ShowPopUp();

            if (airline != null)
            {
               AirlineHelpers.AddSubsidiaryAirline(GameObject.GetInstance().MainAirline, airline, airline.Money,airline.Airports[0]);
               airline.Airports.RemoveAt(0);
             //  airline.Profile.Logo = airline.Profile.Logo;
             //  airline.Profile.Color = airline.Profile.Color;

               this.PageParent.updatePage();

               showSubsidiaries();

               
            }
        }
        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2118"), string.Format(Translator.GetInstance().GetString("MessageBox", "2118", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.MakeSubsidiaryAirlineIndependent(airline);

                    showSubsidiaries();

                    this.PageParent.updatePage();

                }
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            SubsidiaryAirline airline = (SubsidiaryAirline)((Button)sender).Tag;

            if (airline == GameObject.GetInstance().HumanAirline)
            {
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2112"), string.Format(Translator.GetInstance().GetString("MessageBox", "2112", "message"), airline.Profile.Name), WPFMessageBoxButtons.Ok);
            }
            else
            {

                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2111"), string.Format(Translator.GetInstance().GetString("MessageBox", "2111", "message"), airline.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {

                    AirlineHelpers.CloseSubsidiaryAirline(airline);
        
                    showSubsidiaries();

                    this.PageParent.updatePage();

                }
            }
        }
    }
}
