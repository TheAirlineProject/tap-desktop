using TheAirline.Models.Airliners;
using TheAirline.Models.Airlines;
using TheAirline.Models.Airports;
using TheAirline.Views.Airline;

namespace TheAirline.GUIModel.MasterPageModel
{
    using System.Windows;
    using System.Windows.Documents;

    using TheAirline.GUIModel.HelpersModel;
    using TheAirline.GUIModel.PagesModel.AirlinePageModel;
    using TheAirline.GUIModel.PagesModel.AirportPageModel;
    using TheAirline.GUIModel.PagesModel.FleetAirlinerPageModel;

    public partial class standardTemplatesEvents
    {
        #region Methods

        private void Airline_Click(object sender, RoutedEventArgs e)
        {
            var airline = (Airline)((Hyperlink)sender).Tag;
            PageNavigator.NavigateTo(new PageAirline(airline));
        }

        private void Airport_Click(object sender, RoutedEventArgs e)
        {
            var airport = (Airport)((Hyperlink)sender).Tag;
            PageNavigator.NavigateTo(new PageAirport(airport));
        }

        private void Alliance_Click(object sender, RoutedEventArgs e)
        {
        }

        private void lnkAirliner_Click(object sender, RoutedEventArgs e)
        {
            var airliner = (FleetAirliner)((Hyperlink)sender).Tag;

            PageNavigator.NavigateTo(new PageFleetAirliner(airliner));
        }

        #endregion
    }
}