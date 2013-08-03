using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.MasterPageModel
{
    public partial class standardTemplatesEvents
    {
        private void Airport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;
            PageNavigator.NavigateTo(new PageAirport(airport));
        }
    }
}
