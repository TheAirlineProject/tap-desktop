using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel.CountryModel;

namespace TheAirline.GraphicsModel.Converters
{
    //the converter for a boolean to visibility
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Collapsed;
            try
            {
                var x = bool.Parse(value.ToString());
                if (x)
                {
                    rv = Visibility.Visible;
                }
                else
                {
                    rv = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    //the converter for fuel unit to selected unit
    public class FuelUnitConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v;
            else
                return MathHelpers.GallonsToLtr(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for getting the current cultureInfo
    public class CultureInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new CultureInfo(AppSettings.GetInstance().getLanguage().CultureInfo, false);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for l/seat/km to the selected unit
    public class FuelConsumptionToUnitConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v;
            else
                return MathHelpers.LKMToMPG(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a number (in km) to the selected unit
    public class NumberToUnitConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v;
            else
                return MathHelpers.KMToMiles(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a number (in m) to the selected unit
    public class NumberMeterToUnitConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v + " m.";
            else
                return string.Format("{0:0} feet", MathHelpers.MeterToFeet(v));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a string to a language string
    public class StringToLanguageConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String text = (String)value;
            try
            {
                return AppSettings.GetInstance().getLanguage().convert(text);
            }
            catch
            {
                return text;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //the converter for a boolean to bold
    public class BooleanToBold : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FontWeight font = FontWeights.Normal;
            try
            {
                var x = bool.Parse(value.ToString());
                if (x)
                {
                    font = FontWeights.Bold; ;
                }
                else
                {
                    font = FontWeights.Normal;
                }
            }
            catch (Exception)
            {
            }
            return font;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    //the converter for a value to a color for minus
    public class ValueIsMinusConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double amount = double.Parse(value.ToString());

                if (amount >= 0)
                    return Brushes.White;
                else
                    return Brushes.DarkRed;
            }
            catch
            {
                return Brushes.White;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a text with _ to text without
    public class TextUnderscoreConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString().Replace('_', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for an airline to brush
    public class AirlineBrushConverter : IValueConverter
    {

        public object Convert(object value)
        {
            return Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Airline airline = (Airline)value;

            try
            {
                TypeConverter colorConverter = new ColorConverter();
                Color c = (Color)colorConverter.ConvertFromString(airline.Profile.Color);

                return new SolidColorBrush(c);
            }
            catch
            {
                return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a string to a brush
    public class StringToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string color = (string)value;
            try
            {
                TypeConverter colorConverter = new ColorConverter();
                Color c = (Color)colorConverter.ConvertFromString(color);

                return new SolidColorBrush(c);
            }
            catch
            {
                return Brushes.White;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for checking for existing assembly of AirportsCSVReader (Airport Editor visible)
    public class IsAirportEditorVisibleConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                /*
                Assembly assembly = Assembly.LoadFrom(Environment.CurrentDirectory + "\\data\\plugins\\AirportCSVReader.dll");
                if (assembly != null)
                    return Visibility.Collapsed;//Visibility.Visible;
                else
                    return Visibility.Collapsed;
                 * */

                return Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a gridview (listview)
    public class ItemColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            if (index % 2 == 0)
            {
                Brush brush = new SolidColorBrush(Colors.Gray);
                brush.Opacity = 0.50;

                return brush;
            }
            else
            {
                Brush brush = new SolidColorBrush(Colors.DarkGray);
                brush.Opacity = 0.50;

                return brush;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    //converter for airport code
    public class AirportCodeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Airport airport = (Airport)value;

            if (Settings.GetInstance().AirportCodeDisplay == Settings.AirportCode.IATA)
                return airport.Profile.IATACode;
            else
                return airport.Profile.ICAOCode;
        }
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //converter for a price based on the inflation
    public class PriceInflationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long price = System.Convert.ToInt64(value);

            return GeneralHelpers.GetInflationPrice(price);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //converter for a time span to string based on selected language
    public class TimeSpanConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timespan = (TimeSpan)value;

            try
            {
              
                DateTime time = new DateTime(2000, 1, 1, timespan.Hours, timespan.Minutes, 0);

                return time.ToShortTimeString();
            }
            catch
            {
                int hours = timespan.Hours;
                if (hours > 24)
                    hours -= 24;
                if (hours < 0)
                    hours = 24 + hours;

                return new DateTime(2000, 1, 1, hours, Math.Abs(timespan.Minutes), 0).ToShortTimeString();
            }

        }
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    // converter for Translations
    public class TranslatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            string region = parameter as string;
            string uid = "1000";

            return Translator.GetInstance().GetString(region, uid);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //ther converte for the language translation
    public class LanguageTranslationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string uid = parameter.ToString();
            return Translator.GetInstance().GetString("LanguageTranslationConverter", uid);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a country to get current country (used for temporary countries)
    public class CountryCurrentCountryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Union)
                return value;

            Country country = (Country)value;

          
            if (!(country is TemporaryCountry))
            {
                TemporaryCountry tempCountry = TemporaryCountries.GetTemporaryCountry(country,GameObject.GetInstance().GameTime);

                if (tempCountry == null)
                    return country;
                else
                {
                    if (tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime,country) == null)
                        return country;
                    else
                        return tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime,country);
                }
            }
            else
            {
                return ((TemporaryCountry)country).getCurrentCountry(GameObject.GetInstance().GameTime,country);
    
            }
            //return country is TemporaryCountry ? ((TemporaryCountry)country).getCurrentCountry(GameObject.GetInstance().GameTime) : country;
        }
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
