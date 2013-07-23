using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TheAirline.GUIModel.MasterPageModel
{
    /// <summary>
    /// The class for the elements of a standard master page
    /// </summary>
    public class StandardMasterPage : Control
    {
        static StandardMasterPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StandardMasterPage), new FrameworkPropertyMetadata(typeof(StandardMasterPage)));
        }
        //main content area
        public static DependencyProperty MainContentAreaProperty =
            DependencyProperty.Register("MainContentArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate MainContentArea
        {
            get { return (DataTemplate)GetValue(MainContentAreaProperty); }
            set { SetValue(MainContentAreaProperty, value); }
        }
        //main content menu area
        public static DependencyProperty MainContentMenuAreaProperty =
            DependencyProperty.Register("MainContentMenuArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate MainContentMenuArea
        {
            get { return (DataTemplate)GetValue(MainContentMenuAreaProperty); }
            set { SetValue(MainContentMenuAreaProperty, value); }
        }
        //right content area
        public static DependencyProperty RightContentAreaProperty =
             DependencyProperty.Register("RightContentArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate RightContentArea
        {
            get { return (DataTemplate)GetValue(RightContentAreaProperty); }
            set { SetValue(RightContentAreaProperty, value); }
        }
        //left content area
        public static DependencyProperty LeftContentAreaProperty =
            DependencyProperty.Register("LeftContentArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate LeftContentArea
        {
            get { return (DataTemplate)GetValue(LeftContentAreaProperty); }
            set { SetValue(LeftContentAreaProperty, value); }
        }
        //top content area
        public static DependencyProperty HeaderContentAreaProperty =
            DependencyProperty.Register("HeaderContentArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate HeaderContentArea
        {
            get { return (DataTemplate)GetValue(HeaderContentAreaProperty); }
            set { SetValue(HeaderContentAreaProperty, value); }
        }
        //footer content area
        public static DependencyProperty FooterContentAreaProperty =
            DependencyProperty.Register("FooterContentArea", typeof(DataTemplate), typeof(StandardMasterPage));
        public DataTemplate FooterContentArea
        {
            get { return (DataTemplate)GetValue(FooterContentAreaProperty); }
            set { SetValue(FooterContentAreaProperty, value); }
        }
    }
}
