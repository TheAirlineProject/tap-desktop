using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheAirline.GraphicsModel.UserControlModel.MessageBoxModel
{
    /// <summary>
    /// Interaction logic for WPFMessageBox.xaml
    /// </summary>
    public partial class WPFMessageBox : Window
    {
       [ThreadStatic]
        static WPFMessageBox ___MessageBox;

        public WPFMessageBoxResult Result { get; set; }

        public static WPFMessageBoxResult Show(string title, string message, WPFMessageBoxButtons buttons)
        {
            ___MessageBox = new WPFMessageBox();
            MessageBoxViewModel __ViewModel = new MessageBoxViewModel(___MessageBox, title, message,buttons);
            ___MessageBox.DataContext = __ViewModel;
            ___MessageBox.ShowDialog();
            return ___MessageBox.Result;
        }
        public WPFMessageBox()
        {

            this.Result = WPFMessageBoxResult.No;

            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
    }
    
}
