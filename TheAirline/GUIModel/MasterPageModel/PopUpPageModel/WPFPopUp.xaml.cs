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
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;

namespace TheAirline.GUIModel.MasterPageModel.PopUpPageModel
{
    /// <summary>
    /// Interaction logic for WPFPopUp.xaml
    /// </summary>
    public partial class WPFPopUp : Window
    {
        public WPFPopUp()
        {
           
            InitializeComponent();
        }
        public static WPFMessageBoxResult Show(string title, DataTemplate content)
        {
            WPFPopUp popUp = new WPFPopUp();
            popUp.PopUpContent = content;
            popUp.Title = title;
            popUp.ShowDialog();

            return WPFMessageBoxResult.Ok;
            //___MessageBox = new WPFMessageBox();
            //MessageBoxViewModel __ViewModel = new MessageBoxViewModel(___MessageBox, title, message, buttons);
            //_MessageBox.DataContext = __ViewModel;
            //___MessageBox.ShowDialog();
            //return ___MessageBox.Result;
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
        //main content menu area
        public static DependencyProperty PopUpContentProperty =
            DependencyProperty.Register("PopUpContent", typeof(DataTemplate), typeof(WPFPopUp));
        public DataTemplate PopUpContent
        {
            get { return (DataTemplate)GetValue(PopUpContentProperty); }
            set { SetValue(PopUpContentProperty, value); }
        }
    }
}
