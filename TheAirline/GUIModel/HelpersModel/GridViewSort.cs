namespace TheAirline.GUIModel.HelpersModel
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    public class GridViewSort
    {
        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...

        #region Static Fields

        public static readonly DependencyProperty AutoSortProperty = DependencyProperty.RegisterAttached(
            "AutoSort",
            typeof(bool),
            typeof(GridViewSort),
            new UIPropertyMetadata(
                false,
                (o, e) =>
                {
                    var listView = o as ListView;
                    if (listView != null)
                    {
                        if (GetCommand(listView) == null) // Don't change click handler if a command is set
                        {
                            var oldValue = (bool)e.OldValue;
                            var newValue = (bool)e.NewValue;
                            if (oldValue && !newValue)
                            {
                                listView.RemoveHandler(
                                    ButtonBase.ClickEvent,
                                    new RoutedEventHandler(ColumnHeader_Click));
                            }
                            if (!oldValue && newValue)
                            {
                                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }
                        }
                    }
                }));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(GridViewSort),
            new UIPropertyMetadata(
                null,
                (o, e) =>
                {
                    var listView = o as ItemsControl;
                    if (listView != null)
                    {
                        if (!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
                        {
                            if (e.OldValue != null && e.NewValue == null)
                            {
                                listView.RemoveHandler(
                                    ButtonBase.ClickEvent,
                                    new RoutedEventHandler(ColumnHeader_Click));
                            }
                            if (e.OldValue == null && e.NewValue != null)
                            {
                                listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }
                        }
                    }
                }));

        public static readonly DependencyProperty FilteringProperty = DependencyProperty.Register(
            "Filtering",
            typeof(Boolean),
            typeof(GridViewSort));

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null));

        #endregion

        #region Public Methods and Operators

        public static void ApplySort(ICollectionView view, string propertyName)
        {
            var direction = ListSortDirection.Ascending;
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    if (currentSort.Direction == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }
                view.SortDescriptions.Clear();
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        }

        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent != null)
            {
                return (T)parent;
            }
            return null;
        }

        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static Boolean GetFiltering(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(FilteringProperty);
        }

        public static string GetPropertyName(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyNameProperty);
        }

        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static void SetFiltering(DependencyObject obj, Boolean value)
        {
            obj.SetValue(FilteringProperty, value);
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        #endregion

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...

        #region Methods

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                string propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        ICommand command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView.Items, propertyName);
                        }
                    }
                }
            }
        }

        #endregion
    }
}