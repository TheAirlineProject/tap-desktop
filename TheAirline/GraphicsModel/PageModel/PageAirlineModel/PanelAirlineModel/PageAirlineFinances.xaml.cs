using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.GraphicsModel.Converters;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineFinances.xaml
    /// </summary>
    public partial class PageAirlineFinances : Page
    {
        private Airline Airline;
        private TextBlock txtCurrentMoney, txtBalance, txtMarketingBudget;
        private ListBox lbSpecifications, lbLoans;
        //private Slider slMarketingBudget;
        public PageAirlineFinances(Airline airline)
        {
            InitializeComponent();

            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag); 

            this.Airline = airline;



            StackPanel panelFinances = new StackPanel();
            panelFinances.Margin = new Thickness(0, 10, 50, 0);
            /*REMOVE THIS LINE AFTER 0.3.6 PUBLIC RELEASE ************************
            WrapPanel panelMarketingBudget = new WrapPanel();

            double minValue = 0;
            TextBlock txtSliderValue = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(minValue).ToString());
            txtSliderValue.Margin = new Thickness(5, 0, 5, 0);
            txtSliderValue.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            txtSliderValue.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;

            panelFinances.Children.Add(txtSliderValue);

            Slider slMarketingBudget = new Slider();
            slMarketingBudget.Value = 10000;
            slMarketingBudget.Minimum = 1;
            slMarketingBudget.Maximum = 10000000;
            slMarketingBudget.Width = 400;
            slMarketingBudget.Tag = txtSliderValue;
            slMarketingBudget.IsDirectionReversed = false;
            slMarketingBudget.IsSnapToTickEnabled = true;
            slMarketingBudget.IsMoveToPointEnabled = true;
            slMarketingBudget.TickFrequency = 25000;
            slMarketingBudget.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider_ValueChanged);
            slMarketingBudget.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.Both;

            panelFinances.Children.Add(slMarketingBudget);

            Button btnApplyMB = new Button();
            btnApplyMB.Uid = "2001";
            btnApplyMB.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnApplyMB.Height = Double.NaN;
            btnApplyMB.Width = Double.NaN;
            btnApplyMB.Content = Translator.GetInstance().GetString("PageFinances", btnApplyMB.Uid);
            btnApplyMB.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnApplyMB.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnApplyMB.Margin = new Thickness(0, 10, 0, 10);
            btnApplyMB.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnApplyMB.Click += new RoutedEventHandler(btnApplyMB_Click);
            panelFinances.Children.Add(btnApplyMB);

            txtMarketingBudget = new TextBlock();
            txtMarketingBudget.Margin = new Thickness(5, 0, 5, 0);
            txtMarketingBudget.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txtMarketingBudget.Text = Translator.GetInstance().GetString("PageFinances", "1001");
            txtMarketingBudget.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;


            panelFinances.Children.Add(txtMarketingBudget);
            REMOVE THIS LINE AFTER 0.3.6 PUBLIC RELEASE ********************/
            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineFinances", txtHeader.Uid);
            panelFinances.Children.Add(txtHeader);

            TextBlock txtSummary  =new TextBlock();
            txtSummary.Uid = "1002";
            txtSummary.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtSummary.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush");
            txtSummary.FontWeight = FontWeights.Bold;
            txtSummary.Text = Translator.GetInstance().GetString("PageAirlineFinances", txtSummary.Uid);
            panelFinances.Children.Add(txtSummary);



            ListBox lbSummary = new ListBox();
            lbSummary.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSummary.SetResourceReference(ListBox.ItemTemplateProperty, "QuickInfoItem");
            panelFinances.Children.Add(lbSummary);

            //txtCurrentMoney = UICreator.CreateTextBlock(string.Format("{0:c}", this.Airline.Money));
            txtCurrentMoney = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airline.Money).ToString());
            txtCurrentMoney.Foreground = new Converters.ValueIsMinusConverter().Convert(this.Airline.Money, null, null, null) as Brush;
            txtCurrentMoney.ToolTip = UICreator.CreateToolTip("Current expendable cash");

            lbSummary.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineFinances", "1003"), txtCurrentMoney));

            //txtBalance = UICreator.CreateTextBlock(string.Format("{0:c}", this.Airline.Money - this.Airline.StartMoney));
            txtBalance = UICreator.CreateTextBlock(new ValueCurrencyConverter().Convert(this.Airline.Money - this.Airline.StartMoney).ToString());
            txtBalance.Foreground = new Converters.ValueIsMinusConverter().Convert(this.Airline.Money - this.Airline.StartMoney, null, null, null) as Brush;
            lbSummary.Items.Add(new QuickInfoValue(Translator.GetInstance().GetString("PageAirlineFinances", "1004"), txtBalance));
            txtBalance.ToolTip = UICreator.CreateToolTip("Total balance since start of game");

            ContentControl txtSpecifications = new ContentControl();
            txtSpecifications.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtSpecifications.ContentTemplate = this.Resources["SpecsHeader"] as DataTemplate;
            txtSpecifications.Margin = new Thickness(0, 5, 0, 0);

            panelFinances.Children.Add(txtSpecifications);

            lbSpecifications = new ListBox();
            lbSpecifications.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbSpecifications.ItemTemplate = this.Resources["SpecsItem"] as DataTemplate;

            panelFinances.Children.Add(lbSpecifications);

            showSpecifications();

            StackPanel panelLoans = new StackPanel();
            panelLoans.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            panelLoans.Margin = new Thickness(0,5,0,0);

            panelFinances.Children.Add(panelLoans);

            TextBlock txtLoans= new TextBlock();
            txtLoans.Uid = "1005";
            txtLoans.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtLoans.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtLoans.FontWeight = FontWeights.Bold;
            txtLoans.Text = Translator.GetInstance().GetString("PageAirlineFinances", txtLoans.Uid);
            panelLoans.Children.Add(txtLoans);

            ContentControl ccLoans = new ContentControl();
            ccLoans.ContentTemplate = this.Resources["LoansHeader"] as DataTemplate;

            panelLoans.Children.Add(ccLoans);

            lbLoans = new ListBox();
            lbLoans.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbLoans.MaxHeight = GraphicsHelpers.GetContentHeight() / 2 -100;
            lbLoans.ItemTemplate = this.Resources["LoanItem"] as DataTemplate;

            panelLoans.Children.Add(lbLoans);

            Button btnLoan = new Button();
            btnLoan.Uid = "1006";
            btnLoan.SetResourceReference(Button.StyleProperty, "RoundedButton");
            btnLoan.Height = Double.NaN;
            btnLoan.Width = Double.NaN;
            //btnLoan.Visibility = this.Airline.IsHuman ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            btnLoan.Content = Translator.GetInstance().GetString("PageAirlineFinances", btnLoan.Uid);
            btnLoan.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            btnLoan.Margin = new Thickness(0, 10, 0, 0);
            btnLoan.SetResourceReference(Button.BackgroundProperty, "ButtonBrush");
            btnLoan.Click += new RoutedEventHandler(btnLoan_Click);
            panelLoans.Children.Add(btnLoan);

            showLoans(false);

            this.Content = panelFinances;

            //GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlineFinances_OnTimeChanged);

            //this.Unloaded += new RoutedEventHandler(PageAirlineFinances_Unloaded);
        }

        /*REMOVE THIS LINE AFTER 0.3.6 PUBLIC RELEASE *********************
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            TextBlock txtBlock = (TextB
         * slider.Tag;
            txtBlock.Text = new ValueCurrencyConverter().Convert(slider.Value).ToString();

        }

        private void btnApplyMB_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Your marketing budget has been changed!", "Marketing Budget", MessageBoxButton.OK);
            /*News news;
            news.Body = "The marketing department has received and approved your new budget request. It will take place immediatelely. Please make sure you check your advertisement allocation as values have changed.";
            news.Subject = "Marketing Budget";
            news.Type = News.NewsType.Airline_News;
                       

        }
       REMOVE THIS LINE AFTER 0.3.6 PUBLIC RELEASE *************************/
        private void PageAirlineFinances_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirlineFinances_OnTimeChanged);

        }

        private void btnLoan_Click(object sender, RoutedEventArgs e)
        {
            Loan loan = (Loan)PopUpLoan.ShowPopUp();

            if (loan != null)
            {
                this.Airline.addLoan(loan);

            
                AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Loans, loan.Amount);

                showLoans(true);

         
            }
        }

        //shows the loans
        private void showLoans(Boolean forceShow)
        {
            if (this.Airline.Loans.Count != lbLoans.Items.Count || forceShow)
            {
                lbLoans.Items.Clear();

                foreach (Loan loan in this.Airline.Loans.FindAll(l=>l.IsActive))
                    lbLoans.Items.Add(loan);
            }
        }

        //shows the specifications
        private void showSpecifications()
        {
            lbSpecifications.Items.Clear();

            foreach (Invoice.InvoiceType type in Enum.GetValues(typeof(Invoice.InvoiceType)))
            {
                lbSpecifications.Items.Add(new SpecsItem(this.Airline, type));
            }
        }

        private void PageAirlineFinances_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                txtBalance.Text = string.Format("{0:c}", this.Airline.getProfit());
                txtBalance.Foreground = new Converters.ValueIsMinusConverter().Convert(this.Airline.Money - this.Airline.StartMoney, null, null, null) as Brush;

                txtCurrentMoney.Text = string.Format("{0:c}", this.Airline.Money);
                txtCurrentMoney.Foreground = new Converters.ValueIsMinusConverter().Convert(this.Airline.Money, null, null, null) as Brush;            
                showSpecifications();
                showLoans(false);
            }
        }

        private void btnPayLoan_Click(object sender, RoutedEventArgs e)
        {
          
            TextBox txtboxLoan=(TextBox)((Panel)((Button)sender).Parent).FindName("txtboxLoan");
            Loan loan = (Loan)txtboxLoan.Tag;

            if (txtboxLoan.Text.Length > 0)
            {
                double amount = Double.Parse(txtboxLoan.Text);

                if (amount > loan.PaymentLeft || amount <= 0 || amount> this.Airline.Money)
                {
                    WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2104"), Translator.GetInstance().GetString("MessageBox", "2104", "message"), WPFMessageBoxButtons.Ok);
                }
                else
                {
                    WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2105"), string.Format(Translator.GetInstance().GetString("MessageBox", "2105", "message"), amount), WPFMessageBoxButtons.YesNo);

                    if (result == WPFMessageBoxResult.Yes)
                    {
                  
                        AirlineHelpers.AddAirlineInvoice(this.Airline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Loans,Math.Max(-amount,-loan.PaymentLeft));

                        loan.PaymentLeft -= Math.Min(amount, loan.PaymentLeft);

                        showLoans(true);
                    }
                }
            }
        }

        private void txtboxLoan_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Convert.ToDouble(e.Text);
            }
            catch
            {
                e.Handled = true;
            }
        }

        //the class for a category specification
        private class SpecsItem
        {
            public Invoice.InvoiceType InvoiceType { get; set; }
            public Airline Airline { get; set; }
            public double CurrentMonth { get { return getCurrentMonthTotal(); } set { ;} }
            public double LastMonth { get { return getLastMonthTotal(); } set { ;} }
            public double YearToDate { get { return getYearToDateTotal(); } set { ;} }
            public SpecsItem(Airline airline,Invoice.InvoiceType type)
            {
                this.InvoiceType = type;
                this.Airline = airline;
            }

            //returns the total amount for the current month
            public double getCurrentMonthTotal()
            {
                DateTime startDate = new DateTime(GameObject.GetInstance().GameTime.Year,GameObject.GetInstance().GameTime.Month,1);
                return this.Airline.getInvoicesAmount(startDate, GameObject.GetInstance().GameTime, this.InvoiceType);
            }

            //returns the total amount for the last month
            public double getLastMonthTotal()
            {
                DateTime tDate = GameObject.GetInstance().GameTime.AddMonths(-1);
                return this.Airline.getInvoicesAmountMonth(tDate.Year,tDate.Month, this.InvoiceType);
            }

            //returns the total amount for the year to date
            public double getYearToDateTotal()
            {
                return  this.Airline.getInvoicesAmountYear(GameObject.GetInstance().GameTime.Year, this.InvoiceType);
            }
        }
    }
}
