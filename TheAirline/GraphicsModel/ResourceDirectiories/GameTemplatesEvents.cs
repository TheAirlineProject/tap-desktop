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
using System.Windows.Data;
using TheAirline.Model.AirlinerModel.RouteModel;
using TheAirline.Model.GeneralModel;

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
    //the converter for if wifi is enabled
    public class WifiEnabledConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean enabled = GameObject.GetInstance().GameTime.Year >= (int)RouteFacility.FacilityType.WiFi;

            return enabled ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a boolean to visibility
    public class RouteClassFacilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                RouteAirlinerClass rClass = (RouteAirlinerClass)value;

                RouteFacility.FacilityType facilityType = (RouteFacility.FacilityType)Enum.Parse(typeof(RouteFacility.FacilityType), parameter.ToString());

                return rClass.getFacility(facilityType).Name;
            }
            catch
            {
                return "";
            }
     
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
