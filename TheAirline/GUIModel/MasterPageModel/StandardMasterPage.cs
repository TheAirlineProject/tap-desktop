namespace TheAirline.GUIModel.MasterPageModel
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    ///     The class for the elements of a standard master page
    /// </summary>
    public class StandardMasterPage : Control
    {
        //main content area

        #region Static Fields

        public static DependencyProperty ActionMenuAreaProperty = DependencyProperty.Register(
            "ActionMenuArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        public static DependencyProperty FooterContentAreaProperty = DependencyProperty.Register(
            "FooterContentArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        public static DependencyProperty HeaderContentAreaProperty = DependencyProperty.Register(
            "HeaderContentArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        public static DependencyProperty LeftContentAreaProperty = DependencyProperty.Register(
            "LeftContentArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        public static DependencyProperty MainContentAreaProperty = DependencyProperty.Register(
            "MainContentArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        //main content menu area
        public static DependencyProperty MainContentMenuAreaProperty = DependencyProperty.Register(
            "MainContentMenuArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        //right content area
        public static DependencyProperty RightContentAreaProperty = DependencyProperty.Register(
            "RightContentArea",
            typeof(DataTemplate),
            typeof(StandardMasterPage));

        #endregion

        #region Constructors and Destructors

        static StandardMasterPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(StandardMasterPage),
                new FrameworkPropertyMetadata(typeof(StandardMasterPage)));
        }

        #endregion

        #region Public Properties

        public DataTemplate ActionMenuArea
        {
            get
            {
                return (DataTemplate)this.GetValue(ActionMenuAreaProperty);
            }
            set
            {
                this.SetValue(ActionMenuAreaProperty, value);
            }
        }

        public DataTemplate FooterContentArea
        {
            get
            {
                return (DataTemplate)this.GetValue(FooterContentAreaProperty);
            }
            set
            {
                this.SetValue(FooterContentAreaProperty, value);
            }
        }

        public DataTemplate HeaderContentArea
        {
            get
            {
                return (DataTemplate)this.GetValue(HeaderContentAreaProperty);
            }
            set
            {
                this.SetValue(HeaderContentAreaProperty, value);
            }
        }

        public DataTemplate LeftContentArea
        {
            get
            {
                return (DataTemplate)this.GetValue(LeftContentAreaProperty);
            }
            set
            {
                this.SetValue(LeftContentAreaProperty, value);
            }
        }

        public DataTemplate MainContentArea
        {
            get
            {
                return (DataTemplate)this.GetValue(MainContentAreaProperty);
            }
            set
            {
                this.SetValue(MainContentAreaProperty, value);
            }
        }

        public DataTemplate MainContentMenuArea
        {
            get
            {
                return (DataTemplate)this.GetValue(MainContentMenuAreaProperty);
            }
            set
            {
                this.SetValue(MainContentMenuAreaProperty, value);
            }
        }

        public DataTemplate RightContentArea
        {
            get
            {
                return (DataTemplate)this.GetValue(RightContentAreaProperty);
            }
            set
            {
                this.SetValue(RightContentAreaProperty, value);
            }
        }

        #endregion
    }
}