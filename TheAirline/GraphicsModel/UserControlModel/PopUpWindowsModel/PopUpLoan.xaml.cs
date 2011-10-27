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

namespace TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel
{
    /// <summary>
    /// Interaction logic for PopUpLoan.xaml
    /// </summary>
    public partial class PopUpLoan : PopUpWindow
    {
        private ComboBox cbAmount, cbLenght;
        private TextBlock txtMonthlyPayment;
        public static object ShowPopUp()
        {
            PopUpWindow window = new PopUpLoan();
            window.ShowDialog();

            return window.Selected;
        }
        public PopUpLoan()
        {
            InitializeComponent();
            this.Uid = "1000";

            this.Title = Translator.GetInstance().GetString("PopUpLoan", this.Uid);

            this.Width = 400;

            this.Height = 175;

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            StackPanel mainPanel = new StackPanel();
            mainPanel.Margin = new Thickness(10, 10, 10, 10);

            ListBox lbLoan = new ListBox();
            lbLoan.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbLoan.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
        

            cbAmount = new ComboBox();
            cbAmount.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAmount.Width = 150;
            cbAmount.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            cbAmount.ItemStringFormat = "{0:c}";
            cbAmount.SelectionChanged += new SelectionChangedEventHandler(cbLoan_SelectionChanged);

            cbAmount.Items.Add(1000000);
            cbAmount.Items.Add(5000000);
            cbAmount.Items.Add(10000000);
            cbAmount.Items.Add(25000000);
            cbAmount.Items.Add(50000000);
            cbAmount.Items.Add(75000000);
            cbAmount.Items.Add(100000000);

     
            lbLoan.Items.Add(new QuickInfoValue("Amount", cbAmount));



            cbLenght = new ComboBox();
            cbLenght.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbLenght.Width = 150;
            cbLenght.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Right;
            cbLenght.SelectionChanged += new SelectionChangedEventHandler(cbLoan_SelectionChanged);

            cbLenght.Items.Add(1);
            cbLenght.Items.Add(2);
            cbLenght.Items.Add(5);
            cbLenght.Items.Add(10);
            cbLenght.Items.Add(20);
            cbLenght.Items.Add(25);

            lbLoan.Items.Add(new QuickInfoValue("Lenght in years", cbLenght));

            txtMonthlyPayment = new TextBlock();
            txtMonthlyPayment.Width = 150;

            lbLoan.Items.Add(new QuickInfoValue("Monthly payment", txtMonthlyPayment));

            lbLoan.Items.Add(new QuickInfoValue("Rate (Airline value)",UICreator.CreateTextBlock(string.Format("{0}%",GeneralHelpers.GetAirlineLoanRate(GameObject.GetInstance().HumanAirline)))));
            mainPanel.Children.Add(lbLoan);

            mainPanel.Children.Add(createButtonsPanel());

            this.Content = mainPanel;
 
            cbLenght.SelectedIndex = 0;
            cbAmount.SelectedIndex = 0;

          
        }

       

        private void cbLoan_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double amount = Convert.ToDouble(cbAmount.SelectedItem);
            int lenght = Convert.ToInt16(cbLenght.SelectedItem) * 12;

            txtMonthlyPayment.Text = string.Format("{0:c}",MathHelpers.GetMonthlyPayment(amount,GeneralHelpers.GetAirlineLoanRate(GameObject.GetInstance().HumanAirline),lenght));
     
        }
          //creates the button panel
        private WrapPanel createButtonsPanel()
        {
            WrapPanel panelButtons = new WrapPanel();
            panelButtons.Margin = new Thickness(0, 5, 0, 0);

            Button btnOk = new Button();
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
            double amount = Convert.ToDouble(cbAmount.SelectedItem);
            int lenght = Convert.ToInt16(cbLenght.SelectedItem) * 12;

            this.Selected = new Loan(GameObject.GetInstance().GameTime,amount,lenght,GeneralHelpers.GetAirlineLoanRate(GameObject.GetInstance().HumanAirline));
            this.Close();
        }

    }
    
}
