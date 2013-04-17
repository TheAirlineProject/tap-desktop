using System;
using System.IO;
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
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.PageModel.PageFinancesModel
{
    /// <summary>
    /// Interaction logic for PageFinances.xaml
    /// </summary>
    public partial class PageFinances : StandardPage
    {
        private Airline Airline;

        public PageFinances(Airline airline)
        {
            //binds and initializes page
            InitializeComponent();
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);
            SetLocalDefaults(airline);
         
            // binds top level budgets and buttons
            Button btnApply = (Button)this.FindName("buttonApply");
            Button btnReset = (Button)this.FindName("buttonReset");
            Button btn1Year = (Button)this.FindName("_1year");
            Button btn5Year = (Button)this.FindName("_5year");
            Button btn10Year = (Button)this.FindName("_10year");

            Viewbox panelContent = (Viewbox)this.FindName("panelViewbox");
            setMaximums(airline);
            BudgetHelpers.SetDefaults(airline);
            SetLocalDefaults(airline);
            SetOverviewPanel(airline, 1);
            
            //event handlers
            btnApply.Click += new RoutedEventHandler(btnApply_Click);
            btnReset.Click += new RoutedEventHandler(btnReset_Click);
            btn1Year.Click += new RoutedEventHandler(btn1Year_Click);
            btn5Year.Click += new RoutedEventHandler(btn5Year_Click);
            btn10Year.Click += new RoutedEventHandler(btn10Year_Click);

            this.RemoveLogicalChild(panelContent);

            base.setContent(panelContent);

            showPage(this);
        }

        //sets max values
        public void setMaximums(Airline humanAirline)
        {
            marketingSlider.Maximum = securitySlider.Maximum = csSlider.Maximum = (humanAirline.Money * 0.2);
            maintenanceSlider.Maximum = humanAirline.Money * 0.4;
            internetSlider.Maximum = printSlider.Maximum = televisionSlider.Maximum = radioSlider.Maximum = marketingSlider.Maximum / 2;
            overhaulSlider.Maximum = partsSlider.Maximum = engineSlider.Maximum = rsSlider.Maximum = maintenanceSlider.Maximum / 2;
            compSlider.Maximum = promoSlider.Maximum = scSlider.Maximum = prSlider.Maximum = csSlider.Maximum / 2;
            inflightSlider.Maximum = airportSlider.Maximum = baggageSlider.Maximum = itSlider.Maximum = securitySlider.Maximum / 2;
        }

        //sets default values and max values
        public void SetLocalDefaults(Airline humanAirline)
        {
            airportSlider.Value = humanAirline.Budget.AirportBudget;
            compSlider.Value = humanAirline.Budget.CompBudget;
            csSlider.Value = humanAirline.Budget.CSBudget;
            engineSlider.Value = humanAirline.Budget.EnginesBudget;
            baggageSlider.Value = humanAirline.Budget.EquipmentBudget;
            inflightSlider.Value = humanAirline.Budget.InFlightBudget;
            internetSlider.Value = humanAirline.Budget.InternetBudget;
            itSlider.Value = humanAirline.Budget.ITBudget;
            maintenanceSlider.Value = humanAirline.Budget.MaintenanceBudget;
            marketingSlider.Value = humanAirline.Budget.MarketingBudget;
            overhaulSlider.Value = humanAirline.Budget.OverhaulBudget;
            partsSlider.Value = humanAirline.Budget.PartsBudget;
            prSlider.Value = humanAirline.Budget.PRBudget;
            printSlider.Value = humanAirline.Budget.PrintBudget;
            promoSlider.Value = humanAirline.Budget.PromoBudget;
            radioSlider.Value = humanAirline.Budget.RadioBudget;
            rsSlider.Value = humanAirline.Budget.RemoteBudget;
            securitySlider.Value = humanAirline.Budget.SecurityBudget;
            scSlider.Value = humanAirline.Budget.ServCenterBudget;
            televisionSlider.Value = humanAirline.Budget.TelevisionBudget;
        }

        //sets initial overview panel
        private void SetOverviewPanel(Airline humanAirline, int n)
        {
            
            IDictionary<DateTime, AirlineBudget> testBudget = GameObject.GetInstance().HumanAirline.TestBudget;
            AirlineBudget budget = BudgetHelpers.GetOneYearBudget(GameObject.GetInstance().GameTime, n);
            intFleetSizeValue.Text = humanAirline.getFleetSize().ToString();
            intFleetSizeValue1.Text = budget.FleetSize.ToString();
            mCashValue.Text = humanAirline.Money.ToString("C0");
            mCashValue1.Text = budget.Cash.ToString("C0");
            mBudgetValue.Text = BudgetHelpers.SetDefaults(humanAirline).ToString("C0");
            mBudgetValue1.Text = budget.TotalBudget.ToString("C0");
            mrBudgetValue.Text = BudgetHelpers.GetRemainingBudget().ToString("C0");
            mrBudgetValue1.Text = budget.RemainingBudget.ToString("C0");
            //the *0.15 is arbitrary padding
            meoyCashValue.Text = (humanAirline.Money - BudgetHelpers.GetRemainingBudget() - (humanAirline.Budget.TotalBudget * 0.15)).ToString("C0");
            meoyCashValue1.Text = budget.EndYearCash.ToString("C0");
            mAvgAirlinerValue.Text = BudgetHelpers.GetAvgFleetValue().ToString("C0");
            mAvgAirlinerValue1.Text = (budget.FleetValue / budget.FleetSize).ToString("C0");
            mTotalFleetValue.Text = BudgetHelpers.GetFleetValue().ToString("C0");
            mTotalFleetValue1.Text = budget.FleetValue.ToString("C0");
            intTotalEmployees.Text = GameObject.GetInstance().HumanAirline.getNumberOfEmployees().ToString();
            intTotalEmployees1.Text = budget.TotalEmployees.ToString();
            mTotalPayroll.Text = (AirlineHelpers.GetMonthlyPayroll(GameObject.GetInstance().HumanAirline) * 12).ToString("C0");
            mTotalPayroll1.Text = budget.TotalPayroll.ToString("C0");
            intSubsValue.Text = GameObject.GetInstance().HumanAirline.Subsidiaries.Count().ToString("C0");
            intSubsValue1.Text = budget.TotalSubValue.ToString("C0");
            mAvgSubsValue.Text = BudgetHelpers.GetAvgSubValue(humanAirline).ToString("C0");
            mAvgAirlinerValue1.Text = (budget.TotalSubValue / budget.Subsidiaries).ToString("C0");
            //mTotalSubsValue.Text = BudgetHelpers.GetTotalSubValues(humanAirline).ToString("C0");
        }

        public void btn1Year_Click(object sender, RoutedEventArgs e)
        {
            SetOverviewPanel(GameObject.GetInstance().HumanAirline, 1);
        }

        public void btn5Year_Click(object sender, RoutedEventArgs e)
        {
            SetOverviewPanel(GameObject.GetInstance().HumanAirline, 5);
        }

        public void btn10Year_Click(object sender, RoutedEventArgs e)
        {
            SetOverviewPanel(GameObject.GetInstance().HumanAirline, 10);
        }
        
            public void btnApply_Click(object sender, RoutedEventArgs e)
            {
                Airline humanAirline = GameObject.GetInstance().HumanAirline;
                humanAirline.Budget.FleetSize = humanAirline.Fleet.Count();
                humanAirline.Budget.FleetValue = BudgetHelpers.GetFleetValue();
                humanAirline.Budget.Subsidiaries = humanAirline.Subsidiaries.Count();
                humanAirline.Budget.TotalSubValue = BudgetHelpers.GetTotalSubValues(humanAirline);
                humanAirline.Budget.Cash = (long)humanAirline.Money;
                humanAirline.Budget.RemainingBudget = BudgetHelpers.GetRemainingBudget();
                humanAirline.Budget.TotalPayroll = (int)AirlineHelpers.GetMonthlyPayroll(humanAirline);
                humanAirline.Budget.TotalEmployees = humanAirline.getNumberOfEmployees();
                humanAirline.Budget.BudgetExpires = GameObject.GetInstance().GameTime.AddMonths(12);
                humanAirline.Budget.BudgetActive = GameObject.GetInstance().GameTime;
                humanAirline.Budget.AirportBudget = (long)airportSlider.Value;
                humanAirline.Budget.CompBudget = (long)compSlider.Value;
                humanAirline.Budget.CSBudget = (long)csSlider.Value;
                humanAirline.Budget.EnginesBudget = (long)engineSlider.Value;
                humanAirline.Budget.EquipmentBudget = (long)baggageSlider.Value;
                humanAirline.Budget.InFlightBudget = (long)inflightSlider.Value;
                humanAirline.Budget.InternetBudget = (long)internetSlider.Value;
                humanAirline.Budget.ITBudget = (long)itSlider.Value;
                humanAirline.Budget.MaintenanceBudget = (long)maintenanceSlider.Value;
                humanAirline.Budget.MarketingBudget = (long)marketingSlider.Value;
                humanAirline.Budget.OverhaulBudget = (long)overhaulSlider.Value;
                humanAirline.Budget.PartsBudget = (long)partsSlider.Value;
                humanAirline.Budget.PRBudget = (long)prSlider.Value;
                humanAirline.Budget.PrintBudget = (long)printSlider.Value;
                humanAirline.Budget.PromoBudget = (long)promoSlider.Value;
                humanAirline.Budget.RadioBudget = (long)radioSlider.Value;
                humanAirline.Budget.RemoteBudget = (long)rsSlider.Value;
                humanAirline.Budget.SecurityBudget = (long)securitySlider.Value;
                humanAirline.Budget.ServCenterBudget = (long)scSlider.Value;
                humanAirline.Budget.TelevisionBudget = (long)televisionSlider.Value;
                humanAirline.Budget.TotalBudget = (long)securitySlider.Value + (long)marketingSlider.Value + (long)maintenanceSlider.Value + (long)csSlider.Value;
                humanAirline.Budget.EndYearCash = BudgetHelpers.GetEndYearCash(humanAirline.Budget.TotalBudget, (long)humanAirline.Money);
                mBudgetValue.Text = humanAirline.Budget.TotalBudget.ToString("C0");
                mrBudgetValue.Text = BudgetHelpers.GetRemainingBudget().ToString("C0");
                meoyCashValue.Text = BudgetHelpers.GetEndYearCash(humanAirline.Budget.TotalBudget, (long)GameObject.GetInstance().HumanAirline.Money).ToString("C0");
                BudgetHelpers.VerifyValues(humanAirline.Budget);
                SetLocalDefaults(humanAirline);
                
            }
    
            private void btnReset_Click(object sender, RoutedEventArgs e)
            {
                BudgetHelpers.SetDefaults(GameObject.GetInstance().HumanAirline);
                SetLocalDefaults(GameObject.GetInstance().HumanAirline);
                SetOverviewPanel(GameObject.GetInstance().HumanAirline, 1);
            }

        }
    }

