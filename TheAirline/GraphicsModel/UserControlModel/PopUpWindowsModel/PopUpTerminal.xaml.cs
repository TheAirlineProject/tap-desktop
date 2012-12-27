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
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpTerminal.xaml
    /// </summary>
    public partial class PopUpTerminal : PopUpWindow
    {
        private Airport Airport;
        private Terminal Terminal;
        private ucNumericUpDown nudGates;
        private Button btnOk;
        private TextBlock txtDaysToCreate, txtTotalPrice;
        private TextBox txtName;
        //for creating a new terminal
        public static object ShowPopUp(Airport airport)
        {
            PopUpWindow window = new PopUpTerminal(airport);
            window.ShowDialog();

            return window.Selected;
        }
        //for extending an existing terminal
        public static object ShowPopUp(Terminal terminal)
        {
            PopUpWindow window = new PopUpTerminal(terminal);
            window.ShowDialog();

            return window.Selected;
        }
        //for creating a new terminal
        public PopUpTerminal(Airport airport)
        {
            this.Airport = airport;

            InitializeComponent();

            this.Uid = "1000";
            this.Title = Translator.GetInstance().GetString("PopUpTerminal", this.Uid);

            this.Width = 400;

            this.Height = 200;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbTerminal = new ListBox();
            lbTerminal.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminal.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            mainPanel.Children.Add(lbTerminal);

            // chs 28-01-12: added for name of terminal
            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.IsEnabled = this.Terminal == null;
            txtName.Width = 100;
            txtName.Text = this.Terminal == null ? "Terminal" : this.Terminal.Name;
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal","1007"),txtName));
           
            // chs 11-09-11: added numericupdown for selecting number of gates
            nudGates = new ucNumericUpDown();
            nudGates.Height = 30;
            nudGates.MaxValue = 50;
            nudGates.ValueChanged+=new RoutedPropertyChangedEventHandler<decimal>(nudGates_ValueChanged);
            nudGates.MinValue = 1;

            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1001"), nudGates));
            /*
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1002"), UICreator.CreateTextBlock(string.Format("{0:C}",this.Airport.getTerminalPrice()))));
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1003"), UICreator.CreateTextBlock(string.Format("{0:C}",this.Airport.getTerminalGatePrice()))));
            */
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1002"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airport.getTerminalPrice()).ToString())));
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1003"), UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airport.getTerminalGatePrice()).ToString())));

            txtDaysToCreate = UICreator.CreateTextBlock("0 days");
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1004"), txtDaysToCreate));

            txtTotalPrice = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(0).ToString());//UICreator.CreateTextBlock(string.Format("{0:C}", 0));
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1005"), txtTotalPrice));

            mainPanel.Children.Add(createButtonsPanel());

            nudGates.Value = 1;
   

            this.Content = mainPanel;
        }
        // chs 11-04-11: changed for making it possible to extending an existing terminal
        //for extending an existing terminal
        public PopUpTerminal(Terminal terminal)
        {
            this.Terminal = terminal;

            InitializeComponent();

            this.Title = "Extend terminal";

            this.Width = 400;

            this.Height = 175;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbTerminal = new ListBox();
            lbTerminal.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminal.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            mainPanel.Children.Add(lbTerminal);

            txtName = new TextBox();
            txtName.Background = Brushes.Transparent;
            txtName.BorderBrush = Brushes.Black;
            txtName.IsEnabled = this.Terminal == null;
            txtName.Width = 100;
            txtName.Text = this.Terminal == null ? "Terminal" : this.Terminal.Name;
            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1007"), txtName));


            lbTerminal.Items.Add(new QuickInfoValue("Current gates", UICreator.CreateTextBlock(string.Format("{0} gates", this.Terminal.Gates.getGates().Count))));

            // chs 11-09-11: added numericupdown for selecting number of gates
            nudGates = new ucNumericUpDown();
            nudGates.Height = 30;
            nudGates.MaxValue = 50;
            nudGates.MinValue = 1;
            nudGates.ValueChanged += new RoutedPropertyChangedEventHandler<decimal>(nudGates_ValueChanged);

            lbTerminal.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpTerminal", "1001"), nudGates));

            txtDaysToCreate = UICreator.CreateTextBlock("0 days");
            lbTerminal.Items.Add(new QuickInfoValue("Days to extend terminal", txtDaysToCreate));

            txtTotalPrice = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(0).ToString());//UICreator.CreateTextBlock(string.Format("{0:C}", 0));
            lbTerminal.Items.Add(new QuickInfoValue("Price for extending", txtTotalPrice));


            mainPanel.Children.Add(createButtonsPanel());

            nudGates.Value = 1;
       

            this.Content = mainPanel;
        }

        private void nudGates_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
           
            int gates = Convert.ToInt32(nudGates.Value);

            txtDaysToCreate.Text = string.Format("{0} days", getDaysToCreate(gates));

            // chs 11-04-11: changed for making it possible to extending an existing terminal
            long price;
            if (this.Terminal == null)
                price = gates * this.Airport.getTerminalGatePrice() + this.Airport.getTerminalPrice();
            else
                price = gates * this.Terminal.Airport.getTerminalGatePrice();

            txtTotalPrice.Text = new ValueCurrencyConverter().Convert(price).ToString();//string.Format("{0:C}", price);

        }
        //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            btnOk = new Button();
            btnOk.Uid = "100";
            btnOk.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnOk.Height = Double.NaN;
            btnOk.Width = Double.NaN;
            btnOk.Content = Translator.GetInstance().GetString("General", btnOk.Uid);
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnOk.Click += new RoutedEventHandler(btnOk_Click);
    
            panelButtons.Children.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Uid = "101";
            btnCancel.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnCancel.Height = Double.NaN;
            btnCancel.Width = Double.NaN;
            btnCancel.Content = Translator.GetInstance().GetString("General", btnCancel.Uid);
            btnCancel.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnCancel.Margin = new Thickness(5, 0, 0, 0);
            btnCancel.Click += new RoutedEventHandler(btnCancel_Click);

            panelButtons.Children.Add(btnCancel);

            return panelButtons;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt32(nudGates.Value);
            string name = txtName.Text;

            if (name.Length > 0)
            {
                if (this.Terminal == null)
                    this.Selected = new Terminal(this.Airport, GameObject.GetInstance().HumanAirline, name, gates, GameObject.GetInstance().GameTime.Add(new TimeSpan(getDaysToCreate(gates), 0, 0, 0)));
                else
                    this.Selected = gates;
                this.Close();
            }
        }
        //returns the days to create a terminal with a number of gates
        private int getDaysToCreate(int gates)
        {
            if (this.Terminal == null)
                return gates * 10 + 60;
            else
                return gates * 10;

        }
    }
}
