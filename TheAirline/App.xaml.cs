using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Resources;
using System.Globalization;
using TheAirline.Model.GeneralModel;

/*!
 * /brief Namespace of the project
 */
namespace TheAirline
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            AppSettings.Init();
            //FrameworkElement.LanguageProperty.
            /*
            string cultureInfo;
            if (GameObject.GetInstance().Language == null)
                cultureInfo = "da";
            else
                cultureInfo = GameObject.GetInstance().Language.CultureInfo;
            
           

            FrameworkElement.LanguageProperty.OverrideMetadata(

                typeof(FrameworkElement),

                new FrameworkPropertyMetadata(

                    XmlLanguage.GetLanguage(new CultureInfo(cultureInfo).IetfLanguageTag)));//(CultureInfo.CurrentCulture.IetfLanguageTag)));
            */
        }
    }
}
