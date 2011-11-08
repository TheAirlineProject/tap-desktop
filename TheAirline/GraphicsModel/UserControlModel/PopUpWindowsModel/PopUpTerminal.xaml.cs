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

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpTerminal.xaml
    /// </summary>
    public partial class PopUpTerminal : PopUpWindow
    {
        private Airport Airport;
        private Terminal Terminal;
        private TextBox txtGates;
        private Button btnOk;
        private TextBlock txtDaysToCreate, txtTotalPrice;
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

            this.Title = "Create new terminal";

            this.Width = 400;

            this.Height = 200;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbTerminal = new ListBox();
            lbTerminal.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbTerminal.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");

            mainPanel.Children.Add(lbTerminal);

            txtGates = new TextBox();
            txtGates.Background = Brushes.Transparent;
            txtGates.Width = 100;
            txtGates.TextAlignment = TextAlignment.Right;
            txtGates.Margin = new Thickness(2, 0, 0, 0);
            txtGates.PreviewKeyDown += new KeyEventHandler(txtGates_PreviewKeyDown);
            txtGates.PreviewTextInput += new TextCompositionEventHandler(txtGates_PreviewTextInput);
            txtGates.TextChanged += new TextChangedEventHandler(txtGates_TextChanged);

            lbTerminal.Items.Add(new QuickInfoValue("Number of gates", txtGates));

            lbTerminal.Items.Add(new QuickInfoValue("Basic price for terminal", UICreator.CreateTextBlock(string.Format("{0:C}",this.Airport.getTerminalPrice()))));
            lbTerminal.Items.Add(new QuickInfoValue("Price per terminal gate", UICreator.CreateTextBlock(string.Format("{0:C}",this.Airport.getTerminalGatePrice()))));

            txtDaysToCreate = UICreator.CreateTextBlock("0 days");
            lbTerminal.Items.Add(new QuickInfoValue("Days to create terminal", txtDaysToCreate));

            txtTotalPrice = UICreator.CreateTextBlock(string.Format("{0:C}", 0));
            lbTerminal.Items.Add(new QuickInfoValue("Total terminal price", txtTotalPrice));

            mainPanel.Children.Add(createButtonsPanel());

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

            lbTerminal.Items.Add(new QuickInfoValue("Current gates", UICreator.CreateTextBlock(string.Format("{0} gates", this.Terminal.Gates.getGates().Count))));

            txtGates = new TextBox();
            txtGates.Background = Brushes.Transparent;
            txtGates.Width = 100;
            txtGates.TextAlignment = TextAlignment.Right;
            txtGates.Margin = new Thickness(2, 0, 0, 0);
            txtGates.PreviewKeyDown += new KeyEventHandler(txtGates_PreviewKeyDown);
            txtGates.PreviewTextInput += new TextCompositionEventHandler(txtGates_PreviewTextInput);
            txtGates.TextChanged += new TextChangedEventHandler(txtGates_TextChanged);

            lbTerminal.Items.Add(new QuickInfoValue("Number of gates", txtGates));

            txtDaysToCreate = UICreator.CreateTextBlock("0 days");
            lbTerminal.Items.Add(new QuickInfoValue("Days to extend terminal", txtDaysToCreate));

            txtTotalPrice = UICreator.CreateTextBlock(string.Format("{0:C}", 0));
            lbTerminal.Items.Add(new QuickInfoValue("Price for extending", txtTotalPrice));


            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
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
            btnOk.IsEnabled = false;
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
            int gates = Convert.ToInt32(txtGates.Text);
       
            if (this.Terminal == null)
                 this.Selected = new Terminal(this.Airport, GameObject.GetInstance().HumanAirline, gates, GameObject.GetInstance().GameTime.Add(new TimeSpan(getDaysToCreate(gates), 0, 0, 0)));
            else
                this.Selected = gates;
            this.Close();
        }
        private void txtGates_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            if (txtGates.Text.Length == 1 && (e.Key == Key.Delete || e.Key == Key.Back))
                e.Handled = true;
        }

        private void txtGates_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOk.IsEnabled = Convert.ToDouble(txtGates.Text) < 50 && Convert.ToDouble(txtGates.Text) > 0 ? true : false;

            int gates = Convert.ToInt32(txtGates.Text);

            txtDaysToCreate.Text = string.Format("{0} days", getDaysToCreate(gates));

            // chs 11-04-11: changed for making it possible to extending an existing terminal
            long price;
            if (this.Terminal == null)
                price = gates * this.Airport.getTerminalGatePrice() + this.Airport.getTerminalPrice();
            else
                price = gates * this.Terminal.Airport.getTerminalGatePrice();

            txtTotalPrice.Text = string.Format("{0:C}", price);

        }
        private void txtGates_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double number;
            Boolean parseable = double.TryParse(e.Text, out number);

            int length = txtGates.Text.Length;

            e.Handled = !parseable || (length == 0 && number == 0) || length > 2;

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
