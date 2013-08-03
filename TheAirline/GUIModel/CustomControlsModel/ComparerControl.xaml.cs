using System;
using System.Collections;
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

namespace TheAirline.GUIModel.CustomControlsModel
{
    /// <summary>
    /// Interaction logic for ComparerControl.xaml
    /// </summary>
    public partial class ComparerControl : UserControl
    {
        /// <summary>
        /// Identifies the text dependency property.
        /// </summary>
        public static readonly DependencyProperty ValuesProperty =
                                DependencyProperty.Register("Values",
                                typeof(IEnumerable), typeof(ComparerControl));


        [Category("Common Properties")]
        public IEnumerable Values
        {
            get { return (IEnumerable)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }
       
        public static readonly DependencyProperty SelectedCompareTypeProperty =
                                DependencyProperty.Register("SelectedCompareType",
                                typeof(CompareTypes), typeof(ComparerControl));
        
        [Category("Common Properties")]
        public CompareTypes SelectedCompareType
        {
            get { return (CompareTypes)GetValue(SelectedCompareTypeProperty); }
            set { SetValue(SelectedCompareTypeProperty, value); }
        }

        public enum CompareTypes { All, Large_than, Equal_to, Lower_than }
        public List<CompareTypes> Compares 
        {
            get
            {
                return Enum.GetValues(typeof(CompareTypes))
                    .Cast<CompareTypes>()
                    .Select(v => v)
                    .ToList(); 
            }
            set { ;} 
        }

        public object SelectedValue
        {
            get { return cbValues.SelectedItem; }
            set { ;}
        }
        
        public ComparerControl()
        {
            this.SelectedCompareType = CompareTypes.All;
            InitializeComponent();
        }
    }
}
