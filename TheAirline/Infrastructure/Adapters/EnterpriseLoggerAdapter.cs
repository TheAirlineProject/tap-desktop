using Microsoft.Practices.EnterpriseLibrary.Logging;
using Prism.Logging;

namespace TheAirline.Infrastructure.Adapters
{
    public class EnterpriseLoggerAdapter : ILoggerFacade
    {
        public EnterpriseLoggerAdapter()
        {
            Logger.SetLogWriter(new LogWriter(new LoggingConfiguration()));
        }

        public void Log(string message, Category category, Priority priority)
        {
            Logger.Write(message, category.ToString(), (int) priority);
        }
    }
}
