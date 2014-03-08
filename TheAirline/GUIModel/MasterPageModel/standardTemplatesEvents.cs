using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using TheAirline.GUIModel.HelpersModel;
using TheAirline.GUIModel.PagesModel.AirlinePageModel;
using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;

namespace TheAirline.GUIModel.MasterPageModel
{
    public partial class standardTemplatesEvents
    {
        private void Airport_Click(object sender, RoutedEventArgs e)
        {
            Airport airport = (Airport)((Hyperlink)sender).Tag;
            PageNavigator.NavigateTo(new GUIModel.PagesModel.AirportPageModel.PageAirport(airport));
        }
        private void Alliance_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Airline_Click(object sender, RoutedEventArgs e)
        {
            Airline airline = (Airline)((Hyperlink)sender).Tag;
            PageNavigator.NavigateTo(new PageAirline(airline));
        }
        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            FleetAirliner airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageFleetAirliner(airliner));
        }
    }
}
