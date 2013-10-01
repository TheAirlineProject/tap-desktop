using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GUIModel.MasterPageModel.PopUpPageModel
{
    /// <summary>
    /// Interaction logic for WPFPopUpLoadConfiguration.xaml
    /// </summary>
    public partial class WPFPopUpLoadConfiguration : Window
    {
        private readonly IUnityContainer _container;

        public WPFPopUpLoadConfiguration(Configuration.ConfigurationType type)
        {
            InitializeComponent();
          
        }

      
    }
}
