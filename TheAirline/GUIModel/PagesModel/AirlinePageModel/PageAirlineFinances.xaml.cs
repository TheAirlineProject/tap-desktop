namespace TheAirline.GUIModel.PagesModel.AirlinePageModel
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.Helpers;

    /// <summary>
    ///     Interaction logic for PageAirlineFinances.xaml
    /// </summary>
    public partial class PageAirlineFinances : Page
    {
        #region Constructors and Destructors

        public PageAirlineFinances(AirlineMVVM airline)
        {
            this.Airline = airline;
            this.DataContext = this.Airline;

            this.InitializeComponent();
        }

        #endregion

        #region Public Properties

        public AirlineMVVM Airline { get; set; }

        #endregion

        #region Methods

        private void btnApplyLoan_Click(object sender, RoutedEventArgs e)
        {
            double amount = this.slAmount.Value;
            int length = Convert.ToInt16(this.slLenght.Value) * 12;

            var loan = new Loan(GameObject.GetInstance().GameTime, amount, length, this.Airline.LoanRate);

            if (AirlineHelpers.CanApplyForLoan(GameObject.GetInstance().HumanAirline, loan))
            {
                this.Airline.addLoan(loan);

                AirlineHelpers.AddAirlineInvoice(
                    this.Airline.Airline,
                    GameObject.GetInstance().GameTime,
                    Invoice.InvoiceType.Loans,
                    loan.Amount);
            }
            else
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2124"),
                    Translator.GetInstance().GetString("MessageBox", "2124", "message"),
                    WPFMessageBoxButtons.Ok);
            }
        }

        private void btnPayLoan_Click(object sender, RoutedEventArgs e)
        {
            var txtPay = (TextBox)((Button)sender).Tag;
            var loan = (LoanMVVM)txtPay.Tag;

            double amount = Convert.ToDouble(txtPay.Text);

            if (amount <= 0 || amount > this.Airline.Money)
            {
                WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2104"),
                    Translator.GetInstance().GetString("MessageBox", "2104", "message"),
                    WPFMessageBoxButtons.Ok);
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(
                    Translator.GetInstance().GetString("MessageBox", "2105"),
                    string.Format(Translator.GetInstance().GetString("MessageBox", "2105", "message"), amount),
                    WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    double payingAmount = Math.Min(amount, loan.PaymentLeft);

                    loan.payOnLoan(payingAmount);

                    AirlineHelpers.AddAirlineInvoice(
                        this.Airline.Airline,
                        GameObject.GetInstance().GameTime,
                        Invoice.InvoiceType.Loans,
                        -payingAmount);

                    if (loan.PaymentLeft <= 0)
                    {
                        this.Airline.Loans.Remove(loan);
                    }
                }
            }
        }

        #endregion
    }
}