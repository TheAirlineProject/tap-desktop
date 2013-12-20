using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TheAirline.GUIModel.HelpersModel
{

    public static class Extensions
    {
        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler,
    ref T field, T value, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = System.Linq.Expressions.Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }

            field = value;
            return true;
        }
    }
    //the class for some UI helpers
    public class UIHelpers
    {
        //finds the list of radio buttons on a page with a specific group name
        public static List<RadioButton> FindRBChildren(DependencyObject parent, string groupName)
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            List<RadioButton> children = new List<RadioButton>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                RadioButton childType = child as RadioButton;
                if (childType == null)
                {
                    // recursively drill down the tree
                    children.AddRange(FindRBChildren(child, groupName));

                    // If the child is found, break so we do not overwrite the found child. 
                    //if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(groupName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement is RadioButton && ((RadioButton)frameworkElement).GroupName == groupName)
                    {
                        // if the child's name is of the request name
                        children.Add((RadioButton)child);

                    }
                }
                else
                {
                    // child element found.
                    children.Add((RadioButton)child);


                }

            }

            return children;
        }

        //finds an element on a page
        public static T FindChild<T>(DependencyObject parent, string childName)
  where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && (frameworkElement.Name == childName || (frameworkElement.Tag != null && frameworkElement.Tag.ToString() == childName)))
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
        /// <summary>
        /// Base Interface that describes the visual match pattern
        /// Part of the Visual tree walkers
        /// </summary>
        public interface IFinderMatchVisualHelper
        {
            /// <summary>
            /// Does this item match the input visual item
            /// </summary>
            /// <param name="item">Item to check </param>
            /// <returns>True if matched, else false</returns>
            bool DoesMatch(DependencyObject item);

            /// <summary>
            /// Property that defines if we should stop walking the tree after the first match is found
            /// </summary>
            bool StopAfterFirst
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Visual tree walker class that matches based on Type
        /// </summary>
        public class FinderMatchType : IFinderMatchVisualHelper
        {
            private Type _ty = null;
            private bool _stopafterfirst = false;

            public FinderMatchType(Type ty)
            {
                _ty = ty;
            }

            public FinderMatchType(Type ty, bool StopAfterFirst)
            {
                _ty = ty;
                _stopafterfirst = StopAfterFirst;
            }

            public bool DoesMatch(DependencyObject item)
            {
                return _ty.IsInstanceOfType(item);
            }

            public bool StopAfterFirst
            {
                get
                {
                    return _stopafterfirst;
                }
                set
                {
                    _stopafterfirst = value;
                }
            }
        }

        /// <summary>
        /// Visual tree walker function that matches on name of an element
        /// </summary>
        public class FinderMatchName : IFinderMatchVisualHelper
        {
            private String _name = "";

            public FinderMatchName(String name)
            {
                _name = name;
            }

            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is FrameworkElement)
                {
                    if ((item as FrameworkElement).Name == _name) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one of the same name..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }

        /// <summary>
        /// Visual tree helper that matches if the item is focused
        /// </summary>
        public class FinderMatchFocused : IFinderMatchVisualHelper
        {
            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is FrameworkElement)
                {
                    if ((item as FrameworkElement).IsFocused) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one item in focus..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

        }

        /// <summary>
        /// Visual tree helper that matches is the item is an itemshost. Typically used in ItemControls
        /// </summary>
        public class FinderMatchItemHost : IFinderMatchVisualHelper
        {
            public bool DoesMatch(DependencyObject item)
            {
                bool bMatch = false;

                if (item is Panel)
                {
                    if ((item as Panel).IsItemsHost) bMatch = true;
                }

                return bMatch;
            }

            /// <summary>
            /// StopAfterFirst Property.. always true, you can't have more than one item host in an item control..
            /// </summary>
            public bool StopAfterFirst
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }

        }

        /// <summary>
        /// Typically used method that walks down the visual tree from a given point to locate a given match only
        /// once. Typically used with Name/ItemHost etc type matching.
        /// 
        /// Only returns one element
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>Null if no match, else returns the first element that matches</returns>
        public static FrameworkElement SingleFindDownInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            helper.StopAfterFirst = true;

            List<FrameworkElement> lst = FindDownInTree(parent, helper);

            FrameworkElement feRet = null;

            if (lst.Count > 0) feRet = lst[0];

            return feRet;
        }

        /// <summary>
        /// All way visual tree helper that searches UP and DOWN in a tree for the matching pattern.
        /// 
        /// This is used to walk for name matches or type matches typically.
        /// 
        /// Returns only the first matching element
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>Null if no match, else returns the first element that matches</returns>
        public static FrameworkElement SingleFindInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            helper.StopAfterFirst = true;

            List<FrameworkElement> lst = FindInTree(parent, helper);

            FrameworkElement feRet = null;

            if (lst.Count > 0) feRet = lst[0];

            return feRet;
        }

        /// <summary>
        /// Walker that looks down in the visual tree for any matching elements, typically used with Type
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>List of matching FrameworkElements</returns>
        public static List<FrameworkElement> FindDownInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            List<FrameworkElement> lst = new List<FrameworkElement>();

            FindDownInTree(lst, parent, null, helper);

            return lst;
        }

        /// <summary>
        /// Walker that looks both UP and down in the visual tree for any matching elements, typically used with Type
        /// </summary>
        /// <param name="parent">Start point in the tree to search</param>
        /// <param name="helper">Match Helper to use</param>
        /// <returns>List of matching FrameworkElements</returns>
        public static List<FrameworkElement> FindInTree(Visual parent, IFinderMatchVisualHelper helper)
        {
            List<FrameworkElement> lst = new List<FrameworkElement>();

            FindUpInTree(lst, parent, null, helper);

            return lst;
        }

        /// <summary>
        /// Really a helper for FindDownInTree, typically not called directly.
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="parent"></param>
        /// <param name="ignore"></param>
        /// <param name="helper"></param>
        public static void FindDownInTree(List<FrameworkElement> lst, DependencyObject parent, DependencyObject ignore, IFinderMatchVisualHelper helper)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                if (lst.Count > 0 && helper.StopAfterFirst) break;

                DependencyObject visual = VisualTreeHelper.GetChild(parent, i);

                if (visual is FrameworkElement)
                {
                    (visual as FrameworkElement).ApplyTemplate();
                }

                if (helper.DoesMatch(visual))
                {
                    lst.Add(visual as FrameworkElement);
                }

                if (lst.Count > 0 && helper.StopAfterFirst) break;

                if (visual != ignore)
                {
                    FindDownInTree(lst, visual, ignore, helper);
                }
            }
        }

        /// <summary>
        /// Really a helper to look Up in a tree, typically not called directly.
        /// </summary>
        /// <param name="lst"></param>
        /// <param name="parent"></param>
        /// <param name="ignore"></param>
        /// <param name="helper"></param>
        public static void FindUpInTree(List<FrameworkElement> lst, Visual parent, Visual ignore, IFinderMatchVisualHelper helper)
        {
            // First thing to do is find Down in the existing node...
            //FindDownInTree(lst, parent, ignore, helper);

            if (helper.DoesMatch(parent))
            {
                lst.Add(parent as FrameworkElement);
            }


            // Ok, now check to see we are not at a stop.. i.e. got it.
            if (lst.Count > 0 && helper.StopAfterFirst)
            {
                // Hum, don't think we need to do anything here: yet.
            }
            else
            {
                // Ok, now try to get a new parent...
                FrameworkElement feCast = parent as FrameworkElement;
                if (feCast != null)
                {
                    FrameworkElement feNewParent = feCast.Parent as FrameworkElement;
                    if (feNewParent == null || feNewParent == feCast)
                    {
                        // Try to get the templated parent
                        feNewParent = feCast.TemplatedParent as FrameworkElement;
                    }

                    // Now check to see that we have a valid parent
                    if (feNewParent != null && feNewParent != feCast)
                    {
                        // Pass up
                        FindUpInTree(lst, feNewParent, feCast, helper);
                    }
                }
            }
        }

        /// <summary>
        /// Simple form call that returns all elements of a given type down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static List<FrameworkElement> FindElementsOfType(Visual parent, Type ty)
        {
            return FindDownInTree(parent, new FinderMatchType(ty));

        }

        /// <summary>
        /// Simple form call that returns the first element of a given type down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static FrameworkElement FindElementOfType(Visual parent, Type ty)
        {
            return SingleFindDownInTree(parent, new FinderMatchType(ty));
        }

        /// <summary>
        /// Simple form call that returns the first element of a given type up in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public static FrameworkElement FindElementOfTypeUp(Visual parent, Type ty)
        {
            return SingleFindInTree(parent, new FinderMatchType(ty));
        }

        /// <summary>
        /// Helper to pause any media elements down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        public static void TurnOffMediaElements(Visual parent)
        {
            List<FrameworkElement> lst = FindElementsOfType(parent, typeof(MediaElement));

            foreach (FrameworkElement me in lst)
            {
                MediaElement meCast = me as MediaElement;

                if (meCast != null)
                {
                    if (meCast.CanPause)
                    {
                        try
                        {
                            meCast.Pause();
                        }
                        catch (Exception e)
                        {
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Helper to resume playing any media elements down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        public static void TurnOnMediaElements(Visual parent)
        {
            List<FrameworkElement> lst = FindElementsOfType(parent, typeof(MediaElement));

            foreach (FrameworkElement me in lst)
            {
                MediaElement meCast = me as MediaElement;

                if (meCast != null)
                {
                    try
                    {
                        meCast.Play();
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }


        /// <summary>
        /// Helper to find the currently focused element
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static FrameworkElement FindFocusedElement(Visual parent)
        {
            return SingleFindInTree(parent, new FinderMatchFocused());
        }

        /// <summary>
        /// Helper to find a items host (e.g. in a listbox etc) down in the tree
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static FrameworkElement FindItemsHost(Visual parent)
        {
            return SingleFindDownInTree(parent, new FinderMatchItemHost());
        }

        /// <summary>
        /// Helper to find the given named element down in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ElementName"></param>
        /// <returns></returns>
        public static FrameworkElement FindVisualElement(Visual parent, String ElementName)
        {
            return SingleFindDownInTree(parent, new FinderMatchName(ElementName));
        }

        /// <summary>
        /// Helper to find the given named element up in the visual tree
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ElementName"></param>
        /// <returns></returns>
        public static FrameworkElement FindVisualElementUp(Visual parent, String ElementName)
        {
            return SingleFindInTree(parent, new FinderMatchName(ElementName));
        }

        /*!creates a standard text block.
        * */
        public static TextBlock CreateTextBlock(string text)
        {
            TextBlock txtText = new TextBlock();
            txtText.Text = text;


            return txtText;
        }
        //converts coordinates to a map position
        public static Point WorldToTilePos(GeoCoordinate coordinates, int zoom)
        {
            double lon = coordinates.Longitude;
            double lat = coordinates.Latitude;

            Point p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            double maxXValue = Math.Pow(2, zoom);

            if (p.X < 0)
                p.X = maxXValue + p.X;

            if (p.X > maxXValue)
                p.X = p.X - maxXValue;

            return p;
        }


    }
    public class ListBoxItemStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item,
           DependencyObject container)
        {

            Trigger trigger = new Trigger();
            trigger.Property = ListBoxItem.IsFocusedProperty;
            trigger.Value = false;

            Style st = new Style();
            st.TargetType = typeof(ListBoxItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListBoxItem.BackgroundProperty;

            Setter focusVisualSetter = new Setter();
            focusVisualSetter.Property = ListBoxItem.FocusVisualStyleProperty;
            focusVisualSetter.Value = null;

            ListBox listBox =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListBox;
            int index =
                listBox.ItemContainerGenerator.IndexFromContainer(container);
            if (index % 2 == 0)
            {


                Brush brush = new SolidColorBrush(Colors.Gray);
                brush.Opacity = 0.50;

                backGroundSetter.Value = brush;
                
                st.Resources.Add(SystemColors.HighlightBrushKey, brush);
                st.Resources.Add(SystemColors.ControlBrushKey, brush);

            }
            else
            {
                Brush brush = new SolidColorBrush(Colors.DarkGray);
                brush.Opacity = 0.50;

                backGroundSetter.Value = brush;

                st.Resources.Add(SystemColors.HighlightBrushKey, brush);
                st.Resources.Add(SystemColors.ControlBrushKey, brush);
            }
            trigger.Setters.Add(backGroundSetter);

            st.Triggers.Add(trigger);
            st.Setters.Add(backGroundSetter);
            st.Setters.Add(focusVisualSetter);

            return st;
        }
    }
    public class ListBoxItemStyleSelectorTransparent : StyleSelector
    {
        public override Style SelectStyle(object item,
           DependencyObject container)
        {

            Trigger trigger = new Trigger();
            trigger.Property = ListBoxItem.IsFocusedProperty;
            trigger.Value = false;

            Style st = new Style();
            st.TargetType = typeof(ListBoxItem);

            Setter marginSetter = new Setter();
            marginSetter.Property = ListBoxItem.MarginProperty;
            marginSetter.Value = new Thickness(0, 2, 0, 2);

            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListBoxItem.BackgroundProperty;

            Setter focusVisualSetter = new Setter();
            focusVisualSetter.Property = ListBoxItem.FocusVisualStyleProperty;
            focusVisualSetter.Value = null;

            ListBox listBox =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListBox;
            int index =
                listBox.ItemContainerGenerator.IndexFromContainer(container);

            Brush brush = new SolidColorBrush(Colors.Black);
            brush.Opacity = 0.20;

            backGroundSetter.Value = brush;

            st.Resources.Add(SystemColors.HighlightBrushKey, brush);
            st.Resources.Add(SystemColors.ControlBrushKey, brush);
            st.Resources.Add(SystemColors.InactiveSelectionHighlightBrushKey, brush);

            /*
            if (index % 2 == 0)
            {


              
            }
            else
            {
                Brush brush = new SolidColorBrush(Colors.DarkGray);
                brush.Opacity = 0.50;

                backGroundSetter.Value = brush;

                st.Resources.Add(SystemColors.HighlightBrushKey, brush);
                st.Resources.Add(SystemColors.ControlBrushKey, brush);
            }
             * */
            trigger.Setters.Add(backGroundSetter);
         
            st.Triggers.Add(trigger);
         
            st.Setters.Add(backGroundSetter);
            st.Setters.Add(focusVisualSetter);
            st.Setters.Add(marginSetter);

            return st;
        }
    }

}
