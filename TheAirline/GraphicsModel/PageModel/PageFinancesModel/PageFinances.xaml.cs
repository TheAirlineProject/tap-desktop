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
            SetDefaults(airline);
         
            // binds top level budgets and buttons
             Button btnApply = (Button)this.FindName("buttonApply");
            Button btnReset = (Button)this.FindName("buttonReset");

            Viewbox panelContent = (Viewbox)this.FindName("panelViewbox");

            //event handlers
            btnApply.Click += new RoutedEventHandler(btnApply_Click);
            btnReset.Click += new RoutedEventHandler(btnReset_Click);

            this.RemoveLogicalChild(panelContent);

            base.setContent(panelContent);

            showPage(this);
        }


        //sets default values and max values
        public void SetDefaults(Airline humanAirline)
        {
         
            marketingSlider.Maximum = securitySlider.Maximum = csSlider.Maximum = (humanAirline.Money * 0.2);
            maintenanceSlider.Maximum = humanAirline.Money * 0.4;
            internetSlider.Maximum = printSlider.Maximum = televisionSlider.Maximum = radioSlider.Maximum = marketingSlider.Maximum / 2;
            overhaulSlider.Maximum = partsSlider.Maximum = engineSlider.Maximum = rsSlider.Maximum = maintenanceSlider.Maximum / 2;
            compSlider.Maximum = promoSlider.Maximum = scSlider.Maximum = prSlider.Maximum = csSlider.Maximum / 2;
            inflightSlider.Maximum = airportSlider.Maximum = baggageSlider.Maximum = itSlider.Maximum = securitySlider.Maximum / 2;
            marketingSlider.Value = csSlider.Value = securitySlider.Value = humanAirline.Money * 0.05;
            maintenanceSlider.Value = humanAirline.Money * 0.1;
            printSlider.Value = televisionSlider.Value = radioSlider.Value = internetSlider.Value = (marketingSlider.Value / 4);
            overhaulSlider.Value = partsSlider.Value = engineSlider.Value = rsSlider.Value = maintenanceSlider.Value / 4;
            compSlider.Value = promoSlider.Value = scSlider.Value = prSlider.Value = csSlider.Value / 4;
            inflightSlider.Value = airportSlider.Value = baggageSlider.Value = itSlider.Value = securitySlider.Value / 4;
            intFleetSizeValue.Text = humanAirline.getFleetSize().ToString();
            double totalBudget = (marketingSlider.Value + maintenanceSlider.Value + csSlider.Value + securitySlider.Value);
            mCashValue.Text = humanAirline.Money.ToString("C0");
            mBudgetValue.Text = totalBudget.ToString("C0");
            mrBudgetValue.Text = BudgetHelpers.GetRemainingBudget(totalBudget).ToString("C0");
            //the *0.15 is arbitrary padding
            meoyCashValue.Text = (humanAirline.Money - BudgetHelpers.GetRemainingBudget(totalBudget) - (totalBudget * 0.15)).ToString("C0");
        }
        
        private void verifyValues()
            {
                double userMarketingBudget = radioSlider.Value + televisionSlider.Value + printSlider.Value + internetSlider.Value;
                if (userMarketingBudget > marketingSlider.Value)
                {
                    double sumD = userMarketingBudget - marketingSlider.Value;
                    radioSlider.Value -= (sumD / 4);
                    televisionSlider.Value -= (sumD / 4);
                    printSlider.Value -= (sumD / 4);
                    internetSlider.Value -= (sumD / 4);
                }

                double userMaintBudget = partsSlider.Value + engineSlider.Value + overhaulSlider.Value + rsSlider.Value;
                if (userMaintBudget > maintenanceSlider.Value)
                {
                    double sumD = userMaintBudget = maintenanceSlider.Value;
                    partsSlider.Value -= (sumD / 4);
                    overhaulSlider.Value -= (sumD / 4);
                    engineSlider.Value -= (sumD / 4);
                    rsSlider.Value -= (sumD / 4);
                }

                double userCSBudget = scSlider.Value + compSlider.Value + promoSlider.Value + prSlider.Value;
                if (userCSBudget > csSlider.Value)
                {
                    double sumD = userCSBudget - csSlider.Value;
                    scSlider.Value -= (sumD / 4);
                    compSlider.Value -= (sumD / 4);
                    promoSlider.Value -= (sumD / 4);
                    prSlider.Value -= (sumD / 4);
                }

                double userSecurityBudget = airportSlider.Value + baggageSlider.Value + inflightSlider.Value + itSlider.Value;
                if (userSecurityBudget > securitySlider.Value)
                {
                    double sumD = userSecurityBudget - securitySlider.Value;
                    airportSlider.Value -= (sumD / 4);
                    baggageSlider.Value -= (sumD / 4);
                    inflightSlider.Value -= (sumD / 4);
                    itSlider.Value -= (sumD / 4);
                }

            }
            private void btnApply_Click(object sender, RoutedEventArgs e)
            {
                verifyValues();
                GameObject.GetInstance().HumanAirline.Budget.BudgetExpires = GameObject.GetInstance().GameTime.AddMonths(12);
                GameObject.GetInstance().HumanAirline.Budget.AirportBudget = airportSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.CompBudget = compSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.CSBudget = csSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.EnginesBudget = engineSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.EquipmentBudget = baggageSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.InFlightBudget = inflightSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.InternetBudget = internetSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.ITBudget = itSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.MaintenanceBudget = maintenanceSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.MarketingBudget = marketingSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.OverhaulBudget = overhaulSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.PartsBudget = partsSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.PRBudget = prSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.PrintBudget = printSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.PromoBudget = promoSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.RadioBudget = radioSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.RemoteBudget = rsSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.SecurityBudget = securitySlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.ServCenterBudget = scSlider.Value;
                GameObject.GetInstance().HumanAirline.Budget.TelevisionBudget = televisionSlider.Value;
            }


    
            private void btnReset_Click(object sender, RoutedEventArgs e)
            {
                SetDefaults(GameObject.GetInstance().HumanAirline);
            }

        }
    }

