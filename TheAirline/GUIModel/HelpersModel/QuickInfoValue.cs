using System.ComponentModel;
using System.Windows;

namespace TheAirline.GUIModel.HelpersModel
{
    public class QuickInfoValue : FrameworkElement
    {
        #region Static Fields

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof (string),
            typeof (QuickInfoValue));

        #endregion

        #region Public Properties

        [Category("Common Properties")]
        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        //public string Name { get; set; }
        public UIElement Value { get; set; }

        #endregion
    }
}