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
            //var l_CurrentStack = new System.Diagnostics.StackTrace(true);
            
            System.IO.StreamWriter file = new System.IO.StreamWriter(AppSettings.getCommonApplicationDataPath() + "\\theairline.log");
            file.WriteLine("{0}: {1} {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), e.ExceptionObject.ToString());
            file.WriteLine("---------GAME INFORMATION----------");
            file.Write("Gametime: {0}, human airline: {1}",GameObject.GetInstance().GameTime.ToShortDateString(),GameObject.GetInstance().HumanAirline.Profile.Name);
            file.Close();
        }
    }
}
