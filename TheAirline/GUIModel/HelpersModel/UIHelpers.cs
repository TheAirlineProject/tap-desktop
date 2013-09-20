using System;
using System.Collections.Generic;
using System.ComponentModel;
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
