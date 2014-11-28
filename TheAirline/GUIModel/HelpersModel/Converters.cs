namespace TheAirline.GUIModel.HelpersModel
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.AirlinerModel;
    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;
    using TheAirline.Model.GeneralModel.CountryModel;
    using TheAirline.Model.GeneralModel.Helpers;

    //the class for the different converters
    //the converter for a string to a brush
    public class StringToBrushConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (string)value;
            try
            {
                TypeConverter colorConverter = new ColorConverter();
                var c = (Color)colorConverter.ConvertFromString(color);

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

        #endregion
    }
    //the convert for if a value is null
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    //the converter for an airline to brush
    public class AirlineBrushConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var airline = (Airline)value;

            try
            {
                TypeConverter colorConverter = new ColorConverter();
                var c = (Color)colorConverter.ConvertFromString(airline.Profile.Color);

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

        #endregion
    }

    //the converter for airline color
    public class AirlineColorConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (string)value;

            try
            {
                TypeConverter colorConverter = new ColorConverter();
                var baseColor = (Color)colorConverter.ConvertFromString(color);

                Color c2 = Color.FromArgb(25, baseColor.R, baseColor.G, baseColor.B);

                var colorBrush = new LinearGradientBrush();
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    //the converter for a value to the current currency with inflation
    public class ValueCurrencyInflationConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            return new ValueCurrencyConverter().Convert(GeneralHelpers.GetInflationPrice(v));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a value to the current currency
    public class ValueCurrencyConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double v = Double.Parse(value.ToString());

                if (GameObject.GetInstance().CurrencyCountry == null || Double.IsInfinity(v))
                {
                    return string.Format("{0:C}", value);
                }
                CountryCurrency currency =
                    GameObject.GetInstance().CurrencyCountry.getCurrency(GameObject.GetInstance().GameTime);

                if (currency == null)
                {
                    if (Settings.GetInstance().CurrencyShorten)
                    {
                        if (v >= 1000000000 || v <= -1000000000)
                        {
                            return string.Format(
                                "{0:C} {1}",
                                v / 1000000000,
                                Translator.GetInstance().GetString("General", "2001"));
                        }
                        if (v >= 1000000 || v <= -1000000)
                        {
                            return string.Format(
                                "{0:C} {1}",
                                v / 1000000,
                                Translator.GetInstance().GetString("General", "2000"));
                        }
                        return string.Format("{0:C}", value);
                    }
                    return string.Format("{0:C}", value);
                }
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
                        {
                            return string.Format("{0:#,0.##} {2} {1}", sValue, currency.CurrencySymbol, sFormat);
                        }
                        return string.Format("{1}{0:#,0.##} {2}", sValue, currency.CurrencySymbol, sFormat);
                    }
                    if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                    {
                        return string.Format("{0:#,0.##} {1}", currencyValue, currency.CurrencySymbol);
                    }
                    return string.Format("{1}{0:#,0.##}", currencyValue, currency.CurrencySymbol);
                }
                if (currency.Position == CountryCurrency.CurrencyPosition.Right)
                {
                    return string.Format("{0:#,0.##} {1}", currencyValue, currency.CurrencySymbol);
                }
                return string.Format("{1}{0:#,0.##}", currencyValue, currency.CurrencySymbol);
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

        #endregion
    }
    //the converter for a timespan to time string
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value)
        {
            return Convert(value, null, null, null);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            TimeSpan ts = (TimeSpan)value;
    
            if (System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains("H"))
                return new DateTime(1960,1,1).Add(ts).ToString("HH:mm");
            else
                return new DateTime(1960, 1, 1).Add(ts).ToString("h:mm tt");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //the converter for a boolean to visibility
    public class BooleanToVisibility : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Boolean negation = false;
            if (parameter != null && (parameter.ToString() == "!"))
            {
                negation = true;
            }

            var rv = Visibility.Collapsed;
            try
            {
                bool x = bool.Parse(value.ToString());

                if (negation)
                {
                    x = !x;
                }
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
                rv = Visibility.Collapsed;
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        #endregion
    }

    //the converter for translations
    public class TranslatorConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string[] values = parameter.ToString().Split(' ');

                if (values.Length == 1)
                {
                    return Translator.GetInstance().GetString(values[0], "1000");
                }
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

        #endregion
    }

    //the converter for a value to a color for minus
    public class ValueIsMinusConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double amount = double.Parse(value.ToString());

                if (amount >= 0)
                {
                    return Brushes.White;
                }
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

        #endregion
    }

    //the converter for a country to get current country (used for temporary countries)
    public class CountryCurrentCountryConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Union)
            {
                return value;
            }

            var country = (Country)value;

            if (country is TerritoryCountry)
            {
                return ((TerritoryCountry)country).MainCountry;
            }
            if (!(country is TemporaryCountry))
            {
                TemporaryCountry tempCountry = TemporaryCountries.GetTemporaryCountry(
                    country,
                    GameObject.GetInstance().GameTime);

                if (tempCountry == null)
                {
                    return country;
                }
                if (tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime, country) == null)
                {
                    return country;
                }
                return tempCountry.getCurrentCountry(GameObject.GetInstance().GameTime, country);
            }
            return ((TemporaryCountry)country).getCurrentCountry(GameObject.GetInstance().GameTime, country);
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

        #endregion
    }

    //the converter for a text with _ to text without
    public class TextUnderscoreConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().Replace('_', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    //the converter for weight
    public class WeightToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            var weight = (double)value;

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:0} kg", weight);
            }

            return string.Format("{0:0} lbs", MathHelpers.KgToPound(weight));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    //the converter for the cargo size from an airliner
    public class CargoSizeConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cargo = (double)value;

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:0} m3", cargo);
            }
            return string.Format("{0:0} cu feet", MathHelpers.MeterToFeet(cargo) * 10);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for a number (in km) to the selected unit
    public class DistanceToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:#,0} {1}", v, new StringToLanguageConverter().Convert("km."));
            }
            return string.Format(
                "{0:#,0} {1}",
                MathHelpers.KMToMiles(v),
                new StringToLanguageConverter().Convert("km."));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for a small distance (in m) to the selected unit
    public class SmallDistanceToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return v + " m.";
            }
            return string.Format("{0:#,0} feet", MathHelpers.MeterToFeet(v));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for a number (in km/h) to the selected unit
    public class SpeedToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:#,0} {1}", v, new StringToLanguageConverter().Convert("km/t"));
            }
            return string.Format(
                "{0:#,0} {1}",
                MathHelpers.KMToMiles(v),
                new StringToLanguageConverter().Convert("km/t"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for fuel
    public class FuelUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Imperial)
            {
                v = MathHelpers.LtrToGallons(v);
            }

            return string.Format(
                "{0}/{1}",
                new ValueCurrencyConverter().Convert(v),
                new StringToLanguageConverter().Convert("ltr"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //converter for airport code
    public class AirportCodeConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Airport)
            {
                var airport = (Airport)value;

                if (Settings.GetInstance().AirportCodeDisplay == Settings.AirportCode.IATA)
                {
                    return airport.Profile.IATACode;
                }
                return airport.Profile.ICAOCode;
            }
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

        #endregion
    }

    //the converter for a string to a language string
    public class StringToLanguageConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (String)value;

            var translation = AppSettings.GetInstance().getLanguage().convert(text);

            if (translation == null)
                return text;
            else
                return translation;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for l/seat/km to the selected unit
    public class FuelConsumptionToUnitConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value)
        {
            return this.Convert(value, null, null, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v = Double.Parse(value.ToString());

            if (AppSettings.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
            {
                return string.Format("{0:0.00} {1}", v, new StringToLanguageConverter().Convert("l/seat/km"));
            }
            return string.Format(
                "{0:0.000} {1}",
                MathHelpers.LSeatKMToGSeatM(v),
                new StringToLanguageConverter().Convert("l/seat/km"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    //the converter for the airliner class for a fleet airliner
    public class AirlinerClassCodeConverter : IValueConverter
    {
        #region Public Methods and Operators

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Airliner airliner;

            if (value is Airliner)
            {
                airliner = (Airliner)value;
            }
            else
            {
                airliner = Airliners.GetAirliner(value.ToString());
            }

            return string.Join("-", from c in airliner.Classes select AirlinerHelpers.GetAirlinerClassCode(c));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}