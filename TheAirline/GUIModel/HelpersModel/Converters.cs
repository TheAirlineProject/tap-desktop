using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.HelpersModel
{
    //the class for the different converters

    //the converter for translations
    public class TranslatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] values = parameter.ToString().Split(' ');

            return Translator.GetInstance().GetString(values[0], values[1]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
