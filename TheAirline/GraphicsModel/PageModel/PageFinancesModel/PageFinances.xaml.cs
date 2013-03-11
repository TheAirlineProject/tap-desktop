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
    public partial class PageFinances : Page
    {
        private Airline Airline;

        public PageFinances(Airline airline)
        {
            //binds and initializes page
            InitializeComponent();
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);
            SetDefaults(airline);
            Page Budgets = (Page)this.FindName("Budgets");
            Budgets.Width = SystemParameters.PrimaryScreenWidth;
            Budgets.Height = SystemParameters.PrimaryScreenHeight;

            // binds top level budgets and buttons
            Slider marketingSlider = (Slider)this.FindName("marketingSlider");
            Slider maintenanceSlider = (Slider)this.FindName("maintenanceSlider");
            Slider securitySlider = (Slider)this.FindName("securitySlider");
            Slider csSlider = (Slider)this.FindName("csSlider");
            Button btnApply = (Button)this.FindName("buttonApply");
            Button btnReset = (Button)this.FindName("buttonReset");

            //binds all sub-budgets
            Slider printSlider = (Slider)this.FindName("printSlider");
            Slider televisionSlider = (Slider)this.FindName("televisionSlider");
            Slider radioSlider = (Slider)this.FindName("radioSlider");
            Slider internetSlider = (Slider)this.FindName("internetSlider");
            Slider overhaulSlider = (Slider)this.FindName("overhaulSlider");
            Slider partsSlider = (Slider)this.FindName("partsSlider");
            Slider engineSlider = (Slider)this.FindName("engineSlider");
            Slider rsSlider = (Slider)this.FindName("rsSlider");
            Slider inflightSlider = (Slider)this.FindName("inflightSlider");
            Slider airportSlider = (Slider)this.FindName("airportSlider");
            Slider baggageSlider = (Slider)this.FindName("baggageSlider");
            Slider itSlider = (Slider)this.FindName("itSlider");
            Slider compSlider = (Slider)this.FindName("compSlider");
            Slider promoSlider = (Slider)this.FindName("promoSlider");
            Slider scSlider = (Slider)this.FindName("scSlider");
            Slider prSlider = (Slider)this.FindName("prSlider");

            //binds overview panel
            TextBox mCashValue = (TextBox)this.FindName("mCashValue");
            TextBox mBudgetValue = (TextBox)this.FindName("mBudgetValue");
            TextBox mrBudgetValue = (TextBox)this.FindName("mrBudgetValue");
            TextBox meoyCashValue = (TextBox)this.FindName("meoyCashValue");
            TextBox intFleetSizeValue = (TextBox)this.FindName("intFleetSizeValue");
            TextBox mTotalFleetValue = (TextBox)this.FindName("mTotalFleetValue");
            TextBox mAvgAirlinerValue = (TextBox)this.FindName("mAvgAirlinerValue");
            TextBox dblSubsValue = (TextBox)this.FindName("dblSubsValue");
            TextBox mTotalSubsValue = (TextBox)this.FindName("mTotalSubsValue");
            TextBox mAvgSubsValue = (TextBox)this.FindName("mAvgSubsValue");
            TextBox intTotalEmployees = (TextBox)this.FindName("intTotalEmployees");
            TextBox mTotalPayroll = (TextBox)this.FindName("mTotalPayroll");

            //event handlers
            btnApply.Click += new RoutedEventHandler(btnApply_Click);
            btnReset.Click += new RoutedEventHandler(btnReset_Click);
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
            private void btnApply_Click(object sender, RoutedEventArgs e)
            {
                
            }
    
            private void btnReset_Click(object sender, RoutedEventArgs e)
            {
                SetDefaults(GameObject.GetInstance().HumanAirline);
            }

        }
    }

