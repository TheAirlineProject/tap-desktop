using System;
using System.Collections.Generic;
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

namespace TheAirline.GUIModel.PagesModel.OptionsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowOptions.xaml
    /// </summary>
    public partial class PageShowOptions : Page
    {
        public OptionsMVVM Options { get; set; }
        public PageShowOptions()
        {
            this.Options = new OptionsMVVM();
            this.DataContext = this.Options;

            InitializeComponent();
        }
    }
}
