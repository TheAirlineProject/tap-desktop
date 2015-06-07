using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Microsoft.Practices.ServiceLocation;
using TheAirline.Infrastructure.Adapters;

namespace TheAirline
{
    public class AirlineBootstrapper : MefBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<MainWindow>();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(AirlineBootstrapper).Assembly));
        }

        protected override ILoggerFacade CreateLogger()
        {
            return new EnterpriseLoggerAdapter();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
