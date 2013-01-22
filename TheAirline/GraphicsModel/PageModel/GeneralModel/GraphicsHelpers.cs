using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using TheAirline.Model.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.GeneralModel
{
    //the class for some general graphics helpers
    public class GraphicsHelpers
    {
        private static double ContentHeight;
        private static double ContentWidth;
        //converts coordinates to a map position
        public static Point WorldToTilePos(Coordinates coordinates, int zoom)
        {
            double lon = coordinates.Longitude.toDecimal();
            double lat = coordinates.Latitude.toDecimal();

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
        //returns the content width
        public static double GetContentWidth()
        {
            return ContentWidth;
        }
        //sets the content width
        public static void SetContentWidth(double width)
        {
            if (ContentWidth < 1)
                ContentWidth = width;
        }
        //returns the content height
        public static double GetContentHeight()
        {
            return ContentHeight;
        }
        //sets the content height
        public static void SetContentHeight(double height)
        {
            if (ContentHeight < 1)
                ContentHeight = height;
        }
       
    }
}
