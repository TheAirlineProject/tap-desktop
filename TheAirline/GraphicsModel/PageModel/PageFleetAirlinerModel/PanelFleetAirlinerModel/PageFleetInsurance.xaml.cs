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
using TheAirline.Model.AirlinerModel;

namespace TheAirline.GraphicsModel.PageModel.PageFleetAirlinerModel.PanelFleetAirlinerModel
{
    /// <summary>
    /// Interaction logic for PageFleetInsurance.xaml
    /// </summary>
    public partial class PageFleetInsurance : Page
    {
        private FleetAirliner Airliner;
        public PageFleetInsurance(FleetAirliner airliner)
        {
            this.DataContext = airliner;
            this.Airliner = airliner;

            InitializeComponent();

            this.DataContext = this.Airliner;
                 
        }
       
    }
}
