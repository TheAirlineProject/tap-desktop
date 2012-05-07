using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using TheAirline.Model.AirlineModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;
using System.Windows.Documents;
using TheAirline.GraphicsModel.PageModel.PageAirlineModel;
using TheAirline.Model.AirportModel;
using TheAirline.GraphicsModel.PageModel.PageAirportModel;

namespace TheAirline.GraphicsModel.ResourceDirectiories
{
    public partial class GameTemplatesEvents
    {
        private void Airline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirline(airline));
        }
        private void Airport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageAirport(airport));
        }
    }
}
