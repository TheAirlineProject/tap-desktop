using System.Windows;
using TheAirline.Infrastructure;

/*!
 * /brief Namespace of the project
 */

namespace TheAirline
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            AppSettings.Init();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var boot = new AirlineBootstrapper();
            boot.Run();
        }
    }
}