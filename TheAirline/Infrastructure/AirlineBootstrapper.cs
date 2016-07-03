using System.ComponentModel.Composition.Hosting;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Prism.Logging;
using Prism.Mef;
using TheAirline.Infrastructure.Adapters;

namespace TheAirline.Infrastructure
{
    public sealed class AirlineBootstrapper : MefBootstrapper
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

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
    }
}