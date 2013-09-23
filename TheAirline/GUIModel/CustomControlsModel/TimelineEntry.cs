using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TheAirline.GUIModel.CustomControlsModel
{
    public class TimelineEntry
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public Brush Brush{ get; set; }
        public string Text { get; set; }
        public string ToolTip { get; set; }
        public object Source { get; set; }
        public TimelineEntry(object source, TimeSpan startTime, TimeSpan endTime,string text, Brush brush,string tooltip = "")
        {
            this.Source = source;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Brush = brush;
            this.Text = text;
            this.ToolTip = tooltip;

            this.Duration = this.EndTime.Subtract(this.StartTime);
        }
    }
}
