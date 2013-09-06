using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlineModel.SubsidiaryModel;

namespace TheAirline.GUIModel.PagesModel.AirlinesPageModel
{
    /// <summary>
    /// Interaction logic for PageAirlinesStatistics.xaml
    /// </summary>
    public partial class PageAirlinesStatistics : Page
    {
        public ObservableCollection<AirlinesMVVM> AllAirlines { get; set; }
        public PageAirlinesStatistics()
        {
            this.AllAirlines = new ObservableCollection<AirlinesMVVM>();

            foreach (Airline airline in Airlines.GetAllAirlines().FindAll(a => !a.IsSubsidiary).OrderByDescending(a=>a.IsHuman))
            {
                this.AllAirlines.Add(new AirlinesMVVM(airline));

                foreach (SubsidiaryAirline sAirline in airline.Subsidiaries)
                    this.AllAirlines.Add(new AirlinesMVVM(sAirline));
            }

            InitializeComponent();
        }
    }
}
