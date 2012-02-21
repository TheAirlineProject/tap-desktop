using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using System.Windows.Documents;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;

namespace TheAirline.GraphicsModel.ResourceDirectiories
{
    public partial class GameTemplatesEvents
    {
        private void Airline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));
        }
    }
}
