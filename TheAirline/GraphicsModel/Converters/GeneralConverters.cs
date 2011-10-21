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

namespace TheAirline.GraphicsModel.Converters
{
    //the converter for a boolean to visibility
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Visible;
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

            if (GameObject.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v;
            else
                return MathHelpers.LtrToGallons(v);  
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
            return new CultureInfo(GameObject.GetInstance().getLanguage().CultureInfo,false);
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

            if (GameObject.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
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

            if (GameObject.GetInstance().getLanguage().Unit == Language.UnitSystem.Metric)
                return v;
            else
                return MathHelpers.KMToMiles(v);
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
                return GameObject.GetInstance().getLanguage().convert(text);
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
                double amount = (double)value;

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
                Assembly assembly = Assembly.LoadFrom(Environment.CurrentDirectory + "\\data\\plugins\\AirportCSVReader.dll");
                if (assembly != null)
                    return Visibility.Visible;
                else
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
}
