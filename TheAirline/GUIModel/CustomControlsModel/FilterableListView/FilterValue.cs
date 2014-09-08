using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheAirline.GUIModel.CustomControlsModel.FilterableListView
{
    //the class for a filter value
    public class FilterValue : IComparable
    {
        public string Text { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public FilterValue(string text, int minvalue, int maxvalue)
        {
            this.Text = text;
            this.MinValue = minvalue;
            this.MaxValue = maxvalue;
        }

        public int CompareTo(object obj)
        {
            FilterValue value = obj as FilterValue;
            return this.MinValue.CompareTo(value.MinValue);
        }
        public override string ToString()
        {
            return this.Text;
        }
    }
    
}
