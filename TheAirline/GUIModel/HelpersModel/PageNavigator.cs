using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TheAirline.GUIModel.HelpersModel
{
    //the class for the navigator to navigate between pages
    public class PageNavigator
    {
        public static MainWindow MainWindow;
        private static List<UIElement> RefreshElements = new List<UIElement>();

        //navigate to a new page
        public static void NavigateTo(Page page)
        {
            MainWindow.navigateTo(page);
        }
        //navigate back
        public static void NavigateBack()
        {
            MainWindow.navigateBack();
        }
        //navigates forward
        public static void NavigateForward()
        {
            MainWindow.navigateForward();
        }
        //clears the navigator
        public static void ClearNavigator()
        {
            MainWindow.clearNavigator();
        }
        //adds an element to fresh to the list
        public static void AddRefreshElement(UIElement e)
        {
            RefreshElements.Add(e);
        }
        //refreshes the page
        public static void Refresh()
        {
            foreach (UIElement element in RefreshElements)
            {
                element.InvalidateVisual();
            }
        }
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


    }
}
