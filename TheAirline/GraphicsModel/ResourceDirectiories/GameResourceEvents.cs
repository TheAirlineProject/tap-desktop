using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace TheAirline.GraphicsModel.ResourceDirectiories
{
    public partial class GameResourcesEvents
    {
        //hyperlinks
        private void hyperLink_MouseLeave(object sender, MouseEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            link.Foreground = (Brush)link.CommandParameter;
            //link.FontWeight = FontWeights.Normal;
            //     link.FontStretch = FontStretches.UltraExpanded;
            //link.Foreground = Brushes.White;
            //link.TextDecorations = null;
        }

        private void hyperLink_MouseEnter(object sender, MouseEventArgs e)
        {
            Hyperlink link = (Hyperlink)sender;
            link.CommandParameter = link.Foreground;
            link.Foreground = Brushes.Yellow;
            //link.FontWeight = FontWeights.Bold;
            //link.TextDecorations = TextDecorations.Underline;

        }
      
    }
}
