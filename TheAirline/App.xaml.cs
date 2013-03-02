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

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += currentDomain_UnhandledException;
                 
        }

        private static void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("theairline.log");
            file.Write("{0}: {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), e.ExceptionObject.ToString());

            file.Close();
        }
    }
}
