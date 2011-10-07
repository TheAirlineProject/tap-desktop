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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TheAirline.GraphicsModel.UserControlModel
{
    /// <summary>
    /// Interaction logic for ucSelectButton.xaml
    /// </summary>
    public partial class ucSelectButton : Button
    {
        public static readonly DependencyProperty IsSelectedProperty =
                                 DependencyProperty.Register("IsSelected",
                                 typeof(Boolean), typeof(ucSelectButton));


        [Category("Common Properties")]
        public Boolean IsSelected
        {
            get { return (Boolean)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }



        public ucSelectButton()
        {
            InitializeComponent();

            this.Style = this.Resources["ucButtonStyle"] as Style;

            this.Click += new RoutedEventHandler(SelectButton_Click);


        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsSelected = true;

            Panel parent = (Panel)this.Parent;

            foreach (UIElement element in parent.Children)
            {
                if (element != this && element is ucSelectButton)
                    ((ucSelectButton)element).IsSelected = false;
            }
        }
    }
}
