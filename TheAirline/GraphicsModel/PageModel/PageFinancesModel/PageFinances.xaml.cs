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
            InitializeComponent();
            double airlineCash = GameObject.GetInstance().HumanAirline.Money;
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);
            
            // sets max top level budgets
            Slider slMarketingBudget = (Slider)this.FindName("marketingSlider");
            Double maxMarketingBudget = ((airlineCash / 2) / 3 / 12);
            Double maxSecurityBudget = maxMarketingBudget;
            Double maxCSBudget = maxMarketingBudget;
            Double maxMaintBudget = ((airlineCash / 2) / 12);

            // sets maximum marketing sub-budgets
            Slider printSlider = (Slider)this.FindName("printSlider");            
            Slider televisionSlider = (Slider)this.FindName("televisionSlider");            
            Slider radioSlider = (Slider)this.FindName("radioSlider");            
            Slider internetSlider = (Slider)this.FindName("internetSlider");
            internetSlider.Maximum = printSlider.Maximum = televisionSlider.Maximum = radioSlider.Maximum = maxMarketingBudget / 2;

            //sets max maintenance sub-budgets
            Slider overhaulSlider = (Slider)this.FindName("overhaulSlider");
            Slider partsSlider = (Slider)this.FindName("partsSlider");
            Slider engineSlider = (Slider)this.FindName("engineSlider");
            Slider rsSlider = (Slider)this.FindName("rsSlider");
            overhaulSlider.Maximum = partsSlider.Maximum = engineSlider.Maximum = rsSlider.Maximum = maxMaintBudget / 2;

            //sets max security sub-budgets
            Slider inflightSlider = (Slider)this.FindName("inflightSlider");
            Slider airportSlider = (Slider)this.FindName("airportSlider");
            Slider baggageSlider = (Slider)this.FindName("baggageSlider");
            Slider itSlider = (Slider)this.FindName("itSlider");
            inflightSlider.Maximum = airportSlider.Maximum = baggageSlider.Maximum = itSlider.Maximum = (maxSecurityBudget / 2);

            //sets max customer service sub-budgets
            Slider compSlider = (Slider)this.FindName("compSlider");
            Slider promoSlider = (Slider)this.FindName("promoSlider");
            Slider scSlider = (Slider)this.FindName("scSlider");
            Slider prSlider = (Slider)this.FindName("prSlider");
            compSlider.Maximum = promoSlider.Maximum = scSlider.Maximum = prSlider.Maximum = maxCSBudget / 2;

            Button btnApply = (Button)this.FindName("buttonApply");
            Button btnReset = (Button)this.FindName("buttonReset");
            btnApply.Click += new RoutedEventHandler(btnApply_Click);
            btnReset.Click += new RoutedEventHandler(btnReset_Click);
            }

            private void btnApply_Click(object sender, RoutedEventArgs e)
            {
                
            }
    
            private void btnReset_Click(object sender, RoutedEventArgs e)
            {

            }

        }
    }

