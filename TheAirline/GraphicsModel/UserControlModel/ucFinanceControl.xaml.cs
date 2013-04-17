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

namespace TheAirline.GraphicsModel.UserControlModel
{
    /// <summary>
    /// Interaction logic for ucFinanceControl.xaml
    /// </summary>
    public partial class ucFinanceControl : UserControl
    {
        /// <summary>
        /// Identifies the text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
                                DependencyProperty.Register("Text",
                                typeof(string), typeof(ucFinanceControl));


        [Category("Common Properties")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
                                DependencyProperty.Register("Value",
                                typeof(decimal), typeof(ucFinanceControl));

        /// <summary>
        /// Gets or sets the value assigned to the control.
        /// </summary>
        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public ucFinanceControl()
        {
            InitializeComponent();
        }
    }
}
