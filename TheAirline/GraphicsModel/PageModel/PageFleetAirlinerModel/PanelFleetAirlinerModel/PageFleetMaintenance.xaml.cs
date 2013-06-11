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
    /// Interaction logic for PageFleetMaintenance.xaml
    /// </summary>
    public partial class PageFleetMaintenance : Page
    {
        private FleetAirliner Airliner;
        public PageFleetMaintenance(FleetAirliner airliner)
        {
            this.Airliner = airliner;
            InitializeComponent();
        }
    }
}
