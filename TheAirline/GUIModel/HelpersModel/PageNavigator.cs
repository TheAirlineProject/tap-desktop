namespace TheAirline.GUIModel.HelpersModel
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    //the class for the navigator to navigate between pages
    public class PageNavigator
    {
        #region Static Fields

        public static MainWindow MainWindow;

        private static readonly List<UIElement> RefreshElements = new List<UIElement>();

        #endregion

        //navigate to a new page

        //adds an element to fresh to the list

        #region Public Methods and Operators

        public static void AddRefreshElement(UIElement e)
        {
            RefreshElements.Add(e);
        }

        //refreshes the page

        //returns if navigator can go back
        public static Boolean CanGoBack()
        {
            return MainWindow.canGoBack();
        }

        //returns if navigator can go forward
        public static Boolean CanGoForward()
        {
            return MainWindow.canGoBack();
        }

        public static void ClearNavigator()
        {
            MainWindow.clearNavigator();
        }

        public static void NavigateBack()
        {
            MainWindow.navigateBack();
        }

        //navigates forward
        public static void NavigateForward()
        {
            MainWindow.navigateForward();
        }

        public static void NavigateTo(Page page)
        {
            MainWindow.navigateTo(page);
        }

        public static void Refresh()
        {
            foreach (UIElement element in RefreshElements)
            {
                element.InvalidateVisual();
            }
        }

        #endregion
    }
}