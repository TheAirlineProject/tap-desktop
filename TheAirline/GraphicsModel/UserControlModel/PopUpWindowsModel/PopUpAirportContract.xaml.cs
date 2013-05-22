using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.Converters;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpAirportContract.xaml
    /// </summary>
    public partial class PopUpAirportContract : PopUpWindow
    {
        private Airport Airport;
        private ucNumericUpDown nudGates;
        private ComboBox cbLength;
        private TextBlock txtYearlyPayment;
        private AirportContract Contract;
        private Button btnOk;
        private CheckBox cbPayNow;
        public static object ShowPopUp(Airport airport)
        {
            PopUpWindow window = new PopUpAirportContract(airport);
            window.ShowDialog();

            return window.Selected;
        }
        public static object ShowPopUp(AirportContract contract)
        {
            PopUpWindow window = new PopUpAirportContract(contract);
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpAirportContract(AirportContract contract) : this(contract.Airport,contract.Airport.Terminals.getFreeGates() + contract.NumberOfGates)
        {
            this.Contract = contract;
            nudGates.Value = contract.NumberOfGates;
            cbLength.SelectedItem = contract.Length;
        }
        public PopUpAirportContract(Airport airport, int maxgates = -1)
        {
            this.Airport = airport;
            InitializeComponent();

            this.Uid = "1000";

            this.Title = string.Format(Translator.GetInstance().GetString("PopUpAirportContract", this.Uid),this.Airport.Profile.Name);

            this.Width = 350;

            this.Height = 175;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbContent = new ListBox();
            lbContent.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbContent.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            mainPanel.Children.Add(lbContent);

            nudGates = new ucNumericUpDown();
            nudGates.Height = 30;
            nudGates.MaxValue = maxgates == -1 ? this.Airport.Terminals.getFreeGates() : maxgates;
            nudGates.Value = Math.Min(nudGates.MaxValue, 2);
            nudGates.ValueChanged +=nudGates_ValueChanged;
            nudGates.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            nudGates.MinValue = 1;

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpAirportContract","1001"),nudGates));

            cbLength = new ComboBox();
            cbLength.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbLength.Width = 100;
            cbLength.Items.Add(1);
            cbLength.Items.Add(2);
            cbLength.Items.Add(5);
            cbLength.Items.Add(10);
            cbLength.Items.Add(15);
            cbLength.Items.Add(20);
            cbLength.Items.Add(25);
            cbLength.Items.Add(30);
            cbLength.ItemStringFormat = "{0} year(s)";
            cbLength.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            cbLength.SelectionChanged += cbLength_SelectionChanged;
         
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpAirportContract","1002"),cbLength));

            cbPayNow = new CheckBox();
            cbPayNow.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
         
            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpAirportContract","1004"),cbPayNow));

            txtYearlyPayment = new TextBlock();

            lbContent.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PopUpAirportContract", "1003"), txtYearlyPayment));

             mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;

            cbLength.SelectedItem = 1;

         
        }
       
        private void cbLength_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int lenght = Convert.ToInt32(cbLength.SelectedItem);

            setYearlyValue();

            btnOk.IsEnabled = Convert.ToInt32(cbLength.SelectedItem) > 0 && Convert.ToInt32(nudGates.Value) > 0;

            if (lenght <= 2)
            {
                cbPayNow.IsEnabled = false;
                cbPayNow.IsChecked = true;
            }
        }

        private void nudGates_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            setYearlyValue();

            btnOk.IsEnabled = Convert.ToInt32(cbLength.SelectedItem) > 0 && Convert.ToInt32(nudGates.Value) > 0; 
      
        }
        //sets the yearly value
        private void setYearlyValue()
        {
            int gates = Convert.ToInt32(nudGates.Value);
            int lenght = Convert.ToInt32(cbLength.SelectedItem);

            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(this.Airport, gates, lenght);

            txtYearlyPayment.Text = new ValueCurrencyConverter().Convert(yearlyPayment).ToString();

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
            btnOk.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnOk.IsEnabled = false;

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
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            int gates = Convert.ToInt32(nudGates.Value);
            int lenght = Convert.ToInt32(cbLength.SelectedItem);

            double yearlyPayment = AirportHelpers.GetYearlyContractPayment(this.Airport,gates,lenght);

            if (this.Contract == null)
            {
                Boolean payFull = false;
                if (lenght <= 2)
                {
                     payFull = true;
                }
                AirportContract contract = new AirportContract(GameObject.GetInstance().HumanAirline, this.Airport, GameObject.GetInstance().GameTime, gates, lenght, yearlyPayment,payFull);
                this.Selected = contract;
            }
            else
            {
                this.Contract.NumberOfGates = gates;
                this.Contract.Length = lenght;
                this.Contract.YearlyPayment = yearlyPayment;
                this.Contract.ContractDate = GameObject.GetInstance().GameTime;
                this.Contract.ExpireDate = this.Contract.ExpireDate.AddYears(this.Contract.Length);
                this.Selected = this.Contract;
            }
            
            
            
            this.Close();

        }
       
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Selected = null;
            this.Close();

        }
    }
}
