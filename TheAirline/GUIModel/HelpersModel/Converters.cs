using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.AirportModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel;
using TheAirline.Model.GeneralModel.Helpers;

namespace TheAirline.GUIModel.HelpersModel
{
    //the class for the different converters
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
    //the converter for airline color
    public class AirlineColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string color = (string)value;

            try
            {
                TypeConverter colorConverter = new ColorConverter();
                Color baseColor = (Color)colorConverter.ConvertFromString(color);

                Color c2 = Color.FromArgb(25, baseColor.R, baseColor.G, baseColor.B);

                LinearGradientBrush colorBrush = new LinearGradientBrush();
                colorBrush.StartPoint = new Point(0, 0);
                colorBrush.EndPoint = new Point(0, 1);
                colorBrush.GradientStops.Add(new GradientStop(c2, 0.15));
                colorBrush.GradientStops.Add(new GradientStop(baseColor, 0.85));
                colorBrush.GradientStops.Add(new GradientStop(c2, 1));

                return colorBrush;
            }
            catch (Exception)
            {
                return Brushes.DarkRed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a value to the current currency
    public class ValueCurrencyConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double v = Double.Parse(value.ToString());


                if (GameObject.GetInstance().CurrencyCountry == null || Double.IsInfinity(v))
                {
                    return string.Format("{0:C}", value);
                }
                else
                {
                    CountryCurrency currency = GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);


                    if (currency == null)
                    {
                        if (Settings.GetInstance().CurrencyShorten)
                        {
                            if (v >= 1000000000 || v <= -1000000000)
                                return string.Format("{0:C} {1}", v / 1000000000, Translator.GetInstance().GetString("General", "2001"));
                            if (v >= 1000000 || v <= -1000000)
                                return string.Format("{0:C} {1}", v / 1000000, Translator.GetInstance().GetString("General", "2000"));
                            return string.Format("{0:C}", value);
                        }
                        else
                            return string.Format("{0:C}", value);
                    }
                    else
                    {
                        double currencyValue = v * currency.Rate;

                        if (Settings.GetInstance().CurrencyShorten)
                        {
                            if (currencyValue >= 1000000 || currencyValue <= -1000000)
                            {
                                double sValue = currencyValue / 1000000;
                                string sFormat = Translator.GetInstance().GetString("General", "2000");

                                if (currencyValue >= 1000000000 || currencyValue <= -1000000000)
                                {
                                    sValue = currencyValue / 1000000000;
                                    sFormat = Translator.GetInstance().GetString("General", "2001");
                                }

                                if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                                    return string.Format("{0:#,0.##} {2} {1}", sValue, currency.CurrencySymbol, sFormat);
                                else
                                    return string.Format("{1}{0:#,0.##} {2}", sValue, currency.CurrencySymbol, sFormat);

                            }
                            else
                            {
                                if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                                    return string.Format("{0:#,0.##} {1}", currencyValue, currency.CurrencySymbol);
                                else
                                    return string.Format("{1}{0:#,0.##}", currencyValue, currency.CurrencySymbol);

                            }
                        }
                        else
                        {

                            if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                                return string.Format("{0:#,0.##} {1}", currencyValue, currency.CurrencySymbol);
                            else
                                return string.Format("{1}{0:#,0.##}", currencyValue, currency.CurrencySymbol);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Format("{0:C}", value);

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
   
    //the converter for a boolean to visibility
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean negation = false;
            if (parameter != null && (parameter.ToString() == "!"))
                negation = true;

            Visibility rv = Visibility.Collapsed;
            try
            {
                var x = bool.Parse(value.ToString());

                if (negation) x = !x;
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
    //the converter for translations
    public class TranslatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string[] values = parameter.ToString().Split(' ');

                if (values.Length == 1)
                    return Translator.GetInstance().GetString(values[0], "1000");
                else
                    return Translator.GetInstance().GetString(values[0], values[1]);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
     //the converter for a value to a color for minus
    public class ValueIsMinusConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double amount = double.Parse(value.ToString());

                if (amount >= 0)
                    return Brushes.White;
                else
                    return Brushes.Red;
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
    //the converter for a country to get current country (used for temporary countries)
    public class CountryCurrentCountryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Union)
                return value;

            Country country = (Country)value;

            if (country is TerritoryCountry)
            {
                return ((TerritoryCountry)country).MainCountry;
            }
            if (!(country is TemporaryCountry))
            {
                TemporaryCountry tempCountry = TemporaryCountries.GetTemporaryCountry(country, GameObject.GetInstance().GameTime);

                if (tempCountry == null)
                    return country;
                else
                {
                    if (tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime, country) == null)
                        return country;
                    else
                        return tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime, country);
                }
            }
            else
            {
                return ((TemporaryCountry)country).getCurrentCountry(GameObject.GetInstance().GameTime, country);

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
    //the converter for the cargo size from an airliner
    public class CargoSizeConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double cargo = (double)value;

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return string.Format("{0:0} m3", cargo);
            else
                return string.Format("{0:0} cu feet", MathHelpers.MeterToFeet(cargo) * 10);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a number (in km) to the selected unit
    public class DistanceToUnitConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return string.Format("{0:#,0} {1}", v, new StringToLanguageConverter().Convert("km."));
            else
                return string.Format("{0:#,0} {1}", MathHelpers.KMToMiles(v), new StringToLanguageConverter().Convert("km."));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a small distance (in m) to the selected unit
    public class SmallDistanceToUnitConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v + " m.";
            else
                return string.Format("{0:#,0} feet", MathHelpers.MeterToFeet(v));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a number (in km/h) to the selected unit
    public class SpeedToUnitConverter : IValueConverter
    {
       
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return string.Format("{0:#,0} {1}", v, new StringToLanguageConverter().Convert("km/t"));
            else
                return string.Format("{0:#,0} {1}", MathHelpers.KMToMiles(v), new StringToLanguageConverter().Convert("km/t"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for fuel
    public class FuelUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
             double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Imperial)
                v = MathHelpers.LtrToGallons(v);
 
            return string.Format("{0}/{1}",new ValueCurrencyConverter().Convert(v), new StringToLanguageConverter().Convert("ltr"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //converter for airport code
    public class AirportCodeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Airport)
            {
                Airport airport = (Airport)value;

                if (Settings.GetInstance().AirportCodeDisplay == Settings.AirportCode.IATA)
                    return airport.Profile.IATACode;
                else
                    return airport.Profile.ICAOCode;
            }
            else
                return "";
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
                return string.Format("{0:0.00} {1}", v, new StringToLanguageConverter().Convert("l/seat/km"));
            else
                return string.Format("{0:0.00} {1}", MathHelpers.LKMToMPG(v), new StringToLanguageConverter().Convert("l/seat/km"));
            }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for the airliner class for a fleet airliner
    public class AirlinerClassCodeConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Airliner airliner;
            
            if (value is Airliner)
                airliner = (Airliner)value;
            else
                airliner = Airliners.GetAirliner(value.ToString());

            return string.Join("-", from c in airliner.Classes select AirlinerHelpers.GetAirlinerClassCode(c));
   
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
      
    }

}
