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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GUIModel.MasterPageModel.PopUpPageModel
{
    /// <summary>
    /// Interaction logic for WPFRegularPopUp.xaml
    /// </summary>
    public partial class WPFRegularPopUp : Window
    {
         [ThreadStatic]
        static WPFRegularPopUp ___PopUp;

        public WPFPopUpResult Result { get; set; }
      
        public static WPFPopUpResult Show(string title, DataTemplate content, WPFPopUpButtons buttons)
        {
            ___PopUp = new WPFRegularPopUp();
            PopUpViewModel __ViewModel = new PopUpViewModel(___PopUp, title, content,buttons);
            ___PopUp.DataContext = __ViewModel;
            ___PopUp.ShowDialog();
            return ___PopUp.Result;
        }
        public WPFRegularPopUp()
        {

            this.Result = WPFPopUpResult.No;

            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
    
    
}
