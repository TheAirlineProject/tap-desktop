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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlineModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineInsurances.xaml
    /// </summary>
    public partial class PageAirlineInsurances : Page
    {
        public Airline Airline { get; set; }
        public List<AirlineInsurance.InsuranceType> Types { get; set; }
        public PageAirlineInsurances(Airline airline)
        {
            this.Types = new List<AirlineInsurance.InsuranceType>();
            foreach (AirlineInsurance.InsuranceType type in Enum.GetValues(typeof(AirlineInsurance.InsuranceType)))
                this.Types.Add(type);

            AirlineInsurance policy = AirlineInsurance.CreatePolicy(airline, AirlineInsurance.InsuranceType.Full_Coverage, AirlineInsurance.InsuranceScope.Global, AirlineInsurance.PaymentTerms.Annual, true, 10, 20);
            airline.addInsurance(policy);
           
            this.Airline = airline;
         
            InitializeComponent();

            this.DataContext = this;
       
          

        }
    }
}
