using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.GUIModel.PagesModel.GamePageModel
{
    using System.Reflection;

    public class GameVersionMVVM
    {
        public string CurrentVersion
        {
            get
            {
                var assemblyInformationalVersion =
               Assembly.GetExecutingAssembly()
                   .CustomAttributes.FirstOrDefault(
                       att => att.AttributeType == typeof(AssemblyInformationalVersionAttribute));

                if (assemblyInformationalVersion != null)
                {
                    return string.Format("Game Version: {0}", assemblyInformationalVersion.ConstructorArguments[0].ToString().Trim('"'));
                }

                return "unknown version";
            }
        }
    }
}
