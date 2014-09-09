using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TheAirline.GUIModel.HelpersModel
{
    //the class for the navigator to navigate between pages
    public class PageNavigator
    {
        #region Static Fields

        public static MainWindow MainWindow;

        private static readonly List<UIElement> RefreshElements = new List<UIElement>();

        #endregion

        #region Public Methods and Operators

        public static void AddRefreshElement(UIElement e)
        {
            RefreshElements.Add(e);
        }

        //refreshes the page

        //returns if navigator can go back
        public static Boolean CanGoBack()
        {
            return MainWindow.CanGoBack();
        }

        //returns if navigator can go forward
        public static Boolean CanGoForward()
        {
            return MainWindow.CanGoBack();
        }

        public static void ClearNavigator()
        {
            MainWindow.ClearNavigator();
        }

        public static void NavigateBack()
        {
            MainWindow.NavigateBack();
        }

        //navigates forward
        public static void NavigateForward()
        {
            MainWindow.NavigateForward();
        }

        public static void NavigateTo(Page page)
        {
            MainWindow.NavigateTo(page);
        }

        public static void Refresh()
        {
            foreach (UIElement element in RefreshElements)
            {
                element.InvalidateVisual();
            }
        }

        #endregion

        //navigate to a new page

        //adds an element to fresh to the list
    }
}