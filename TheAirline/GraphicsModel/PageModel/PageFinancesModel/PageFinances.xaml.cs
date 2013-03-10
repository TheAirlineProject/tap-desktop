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
            this.Language = XmlLanguage.GetLanguage(new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, true).IetfLanguageTag);

            this.Airline = airline;
            Page page = null;
          using (FileStream fs = new FileStream("TheAirline\\GraphicsModel\\PageModel\\PageFinancesModel\\PageFinances.xaml", FileMode.Open, FileAccess.Read))
            {
            page = (Page)XamlReader.Load(fs);
            }

          string airlineCash = GameObject.GetInstance().HumanAirline.Money.ToString();
          TextBox cashValue = (TextBox)page.FindName("cashValue");
          cashValue.DataContext = airlineCash;
        }
    }
}
