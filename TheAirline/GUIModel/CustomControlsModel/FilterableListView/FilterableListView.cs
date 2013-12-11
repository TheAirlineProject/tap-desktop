using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using TheAirline.GUIModel.HelpersModel;

namespace TheAirline.GUIModel.CustomControlsModel.FilterableListView
{
    public class FilterableListView : SortableListView
    {
        #region dependency properties

        /// <summary>
        /// The style applied to the filter button when it is an active state
        /// </summary>
        public Style FilterButtonActiveStyle
        {
            get { return (Style)GetValue(FilterButtonActiveStyleProperty); }
            set { SetValue(FilterButtonActiveStyleProperty, value); }
        }

        public static readonly DependencyProperty FilterButtonActiveStyleProperty =
                       DependencyProperty.Register("FilterButtonActiveStyle", typeof(Style), typeof(FilterableListView), new UIPropertyMetadata(null));

        /// <summary>
        /// The style applied to the filter button when it is an inactive state
        /// </summary>
        public Style FilterButtonInactiveStyle
        {
            get { return (Style)GetValue(FilterButtonInactiveStyleProperty); }
            set { SetValue(FilterButtonInactiveStyleProperty, value); }
        }

        public static readonly DependencyProperty FilterButtonInactiveStyleProperty =
                       DependencyProperty.Register("FilterButtonInActiveStyle", typeof(Style), typeof(FilterableListView), new UIPropertyMetadata(null));


        #endregion

        public static readonly ICommand ShowFilter = new RoutedCommand();

        private ArrayList filterList;

        #region inner classes

        /// <summary>
        /// A simple data holder for passing information regarding filter clicks
        /// </summary>
        struct FilterStruct
        {
            public Button button;
            public FilterItem[] values;
            public String property;
            public FilterStruct(String property, Button button, FilterItem[] values)
            {
                this.values = values;
                this.button = button;
                this.property = property;
            }

        }

        /// <summary>
        /// The items which are bound to the drop down filter list
        /// </summary>
        private class FilterItem : IComparable
        {
            /// <summary>
            /// The filter item instance
            /// </summary>
            private Object item;

            public Object Item
            {
                get { return item; }
                set { item = value; }
            }

            /// <summary>
            /// The item viewed in the filter drop down list. Typically this is the same as the item
            /// property, however if item is null, this has the value of "[empty]"
            /// </summary>
            private Object itemView;

            public Object ItemView
            {
                get { return itemView; }
                set { itemView = value; }
            }

            public Boolean IsChecked { get; set; }
            public FilterItem(IComparable item)
            {
                this.item = this.itemView = item;
                if (item == null)
                {
                    itemView = "[empty]";
                }
            }

            public override int GetHashCode()
            {
                return item != null ? item.GetHashCode() : 0;
            }

            public override bool Equals(object obj)
            {
                FilterItem otherItem = obj as FilterItem;
                if (otherItem != null)
                {
                    if (otherItem.Item == this.Item)
                    {
                        return true;
                    }
                }
                return false;
            }

            public int CompareTo(object obj)
            {
                FilterItem otherFilterItem = (FilterItem)obj;

                if (this.Item == null && obj == null)
                {
                    return 0;
                }
                else if (otherFilterItem.Item != null && this.Item != null)
                {
                    return ((IComparable)item).CompareTo((IComparable)otherFilterItem.item);
                }
                else
                {
                    return -1;
                }
            }

        }

        #endregion

        private Hashtable currentFilters = new Hashtable();
        public Hashtable getCurrentFilters()
        {
            return currentFilters;
        }
        public void setCurrentFilters(Hashtable filters)
        {
            currentFilters = filters;

            ApplyCurrentFilters();
        }
        private void AddFilter(String property, FilterItem value, Button button)
        {
            if (currentFilters.ContainsKey(property))
            {
                currentFilters.Remove(property);
            }
            currentFilters.Add(property, new FilterStruct(property, button, new FilterItem[] { value }));
        }
        private void AddFilters(String property, FilterItem[] values, Button button)
        {
            if (currentFilters.ContainsKey(property))
            {
                currentFilters.Remove(property);
            }
            currentFilters.Add(property, new FilterStruct(property, button, values));

        }
        private bool IsPropertyFiltered(String property)
        {
            foreach (String filterProperty in currentFilters.Keys)
            {
                FilterStruct filter = (FilterStruct)currentFilters[filterProperty];
                if (filter.property == property)
                    return true;
            }

            return false;
        }




        public FilterableListView()
        {
            CommandBindings.Add(new CommandBinding(ShowFilter, ShowFilterCommand));
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //Uri uri = new Uri("MasterPageModel/ComponentsModel/FilterableListView/FilterListViewDictionary.xaml", UriKind.Relative);<
            //dictionary = Application.LoadComponent(uri) as ResourceDictionary;

            // cast the ListView's View to a GridView
            GridView gridView = this.View as GridView;
            if (gridView != null)
            {
                // apply the data template, that includes the popup, button etc ... to each column
                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    if (gridViewColumn is SortableGridViewColumn && !((SortableGridViewColumn)gridViewColumn).CanFilter)
                        gridViewColumn.HeaderTemplate = (DataTemplate)dictionary["SortableGridHeaderTemplate"];
                    else
                        gridViewColumn.HeaderTemplate = (DataTemplate)dictionary["FilterGridHeaderTemplate"];



                }
            }

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // ensure that the custom inactive style is applied
            if (FilterButtonInactiveStyle != null)
            {
                List<FrameworkElement> columnHeaders = UIHelpers.FindElementsOfType(this, typeof(GridViewColumnHeader));

                foreach (FrameworkElement columnHeader in columnHeaders)
                {
                    Button button = (Button)UIHelpers.FindElementOfType(columnHeader, typeof(Button));
                    if (button != null)
                    {
                        button.Style = FilterButtonInactiveStyle;
                    }
                }
            }

        }


        /// <summary>
        /// Handles the ShowFilter command to populate the filter list and display the popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowFilterCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;

            if (button != null)
            {
                // navigate up to the header
                GridViewColumnHeader header = (GridViewColumnHeader)UIHelpers.FindElementOfTypeUp(button, typeof(GridViewColumnHeader));

                // then down to the popup
                Popup popup = (Popup)UIHelpers.FindElementOfType(header, typeof(Popup));

                if (popup != null)
                {
                    // find the property name that we are filtering
                    SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
                    String propertyName = column.SortPropertyName;

                    
                    // clear the previous filter
                    if (filterList == null)
                    {
                        filterList = new ArrayList();
                    }
                    filterList.Clear();

                    // if this property is currently being filtered, provide an option to clear the filter.
                    if (IsPropertyFiltered(propertyName))
                    {
                        filterList.Add(new FilterItem("Clear"));
                    }
                    else
                    {
                        bool containsNull = false;

                        //PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(Airport))[propertyName];



                        // iterate over all the objects in the list
                        foreach (Object item in Items)
                        {
                            object value = getPropertyValue(item, propertyName);


                            if (value != null)
                            {
                                FilterItem filterItem = new FilterItem(value as IComparable);

                                Boolean contains = filterList.Cast<FilterItem>().ToList().Exists(i=>i.Item.ToString() == filterItem.Item.ToString());

                                if (!filterList.Contains(filterItem) && !contains)
                                {
                                    filterList.Add(filterItem);
                                }

                            }
                            else
                            {
                                containsNull = true;
                            }
                        }

                        filterList.Sort();

                        if (containsNull)
                        {
                            filterList.Add(new FilterItem(null));
                        }
                    }

                    // open the popup to display this list
                    popup.DataContext = filterList;
                    CollectionViewSource.GetDefaultView(filterList).Refresh();
                    popup.IsOpen = true;

                    // connect to the selection change event
                    ListView listView = UIHelpers.FindChild<ListView>(popup.Child, "filterList");
                    //listView.SelectionChanged += SelectionChangedHandler;

                    Button btnOk = UIHelpers.FindChild<Button>(popup.Child, "btnOk");
                    btnOk.Click += btnOk_Click;

                    Button btnCancel = UIHelpers.FindChild<Button>(popup.Child, "btnCancel");
                    btnCancel.Click += btnCancel_Click;
                }
            }
        }




        //returns the value of a property for an object
        private object getPropertyValue(object obj, string propertyName)
        {
            object value;
            if (propertyName.Contains('.'))
            {
                string[] split = propertyName.Split('.');

                object tObj = obj.GetType().GetProperty(split[0]).GetValue(obj, null);

                string propertyRest = propertyName.Substring(split[0].Length + 1);

                return getPropertyValue(tObj, propertyRest);

            }
            else
                value = obj.GetType().GetProperty(propertyName).GetValue(obj, null);

            return value;
        }


        /// <summary>
        /// Applies the current filter to the list which is being viewed
        /// </summary>
        private void ApplyCurrentFilters()
        {
            if (currentFilters.Count == 0)
            {
                Items.Filter = null;
                return;
            }

            // construct a filter and apply it               
            Items.Filter = delegate(object item)
            {
                // when applying the filter to each item, iterate over all of
                // the current filters
                bool match = true;
                foreach (FilterStruct filter in currentFilters.Values)
                {
                    // obtain the value for this property on the item under test
                    //PropertyDescriptor filterPropDesc = TypeDescriptor.GetProperties(typeof(object))[filter.property];
                    object itemValue = getPropertyValue(item, filter.property);// filterPropDesc.GetValue((object)item);

                    if (itemValue != null)
                    {
                        Boolean isFilterMatch = false;
                        // check to see if it meets our filter criteria
                        foreach (FilterItem value in filter.values)
                        {
                            if (itemValue.Equals(value.Item))
                                isFilterMatch = true;
                        }

                        if (!isFilterMatch)
                            match = false;

                    }
                    else
                    {
                        if (filter.values.ToList().Exists(v => v.Item != null))
                            match = false;
                    }
                }
                return match;
            };
        }
        /// <summary>
        /// Handles the button click on the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Button btnCancel = (Button)sender;
            ListView filterListView = (ListView)btnCancel.Tag;

            Popup popup = (Popup)UIHelpers.FindElementOfTypeUp(filterListView, typeof(Popup));
            popup.IsOpen = false;
        }
        /// <summary>
        /// Handles the button click on the ok button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Button btnOk = (Button)sender;
            ListView filterListView = (ListView)btnOk.Tag;

            // navigate up to the header to obtain the filter property name
            GridViewColumnHeader header = (GridViewColumnHeader)UIHelpers.FindElementOfTypeUp(filterListView, typeof(GridViewColumnHeader));

            SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
            String currentFilterProperty = column.SortPropertyName;

            var items = new List<FilterItem>();
            foreach (object o in filterListView.Items)
            {
                var item = (FilterItem)o;

                if (item.IsChecked)
                    items.Add(item);
            }

            if (items.Exists(i => i.ItemView.Equals("Clear")))
            {
                if (currentFilters.ContainsKey(currentFilterProperty))
                {
                    FilterStruct filter = (FilterStruct)currentFilters[currentFilterProperty];
                    filter.button.ContentTemplate = (DataTemplate)dictionary["filterButtonInactiveTemplate"];
                    if (FilterButtonInactiveStyle != null)
                    {
                        filter.button.Style = FilterButtonInactiveStyle;
                    }
                    currentFilters.Remove(currentFilterProperty);
                }

                ApplyCurrentFilters();
            }
            else
            {
                // find the button and apply the active style
                Button button = (Button)UIHelpers.FindVisualElement(header, "filterButton");
                button.ContentTemplate = (DataTemplate)dictionary["filterButtonActiveTemplate"];

                if (FilterButtonActiveStyle != null)
                {
                    button.Style = FilterButtonActiveStyle;
                }

                AddFilters(currentFilterProperty, items.ToArray(), button);
                ApplyCurrentFilters();
            }
            // navigate up to the popup and close it
            Popup popup = (Popup)UIHelpers.FindElementOfTypeUp(filterListView, typeof(Popup));
            popup.IsOpen = false;
        }
        /// <summary>
        /// Handles the selection change event from the filter popup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectionChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            // obtain the term to filter for
            ListView filterListView = (ListView)sender;
            FilterItem filterItem = (FilterItem)filterListView.SelectedItem;

            // navigate up to the header to obtain the filter property name
            GridViewColumnHeader header = (GridViewColumnHeader)UIHelpers.FindElementOfTypeUp(filterListView, typeof(GridViewColumnHeader));

            SortableGridViewColumn column = (SortableGridViewColumn)header.Column;
            String currentFilterProperty = column.SortPropertyName;

            if (filterItem == null)
                return;

            // determine whether to clear the filter for this column
            if (filterItem.ItemView.Equals("Clear"))
            {
                if (currentFilters.ContainsKey(currentFilterProperty))
                {
                    FilterStruct filter = (FilterStruct)currentFilters[currentFilterProperty];
                    filter.button.ContentTemplate = (DataTemplate)dictionary["filterButtonInactiveTemplate"];
                    if (FilterButtonInactiveStyle != null)
                    {
                        filter.button.Style = FilterButtonInactiveStyle;
                    }
                    currentFilters.Remove(currentFilterProperty);
                }

                ApplyCurrentFilters();
            }
            else
            {
                // find the button and apply the active style
                Button button = (Button)UIHelpers.FindVisualElement(header, "filterButton");
                button.ContentTemplate = (DataTemplate)dictionary["filterButtonActiveTemplate"];

                if (FilterButtonActiveStyle != null)
                {
                    button.Style = FilterButtonActiveStyle;
                }

                AddFilter(currentFilterProperty, filterItem, button);
                ApplyCurrentFilters();
            }

            // navigate up to the popup and close it
            Popup popup = (Popup)UIHelpers.FindElementOfTypeUp(filterListView, typeof(Popup));
            popup.IsOpen = false;
        }
    }
}
