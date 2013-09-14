using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace TheAirline.GUIModel.MasterPageModel
{
    public partial class standardStylesEvents
    {
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
        private void NumericTextBox_Input(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !IsValidTextNumber(e.Text);
            }
            catch (Exception)
            {

            }
        }
        private void NumericTextBox_CheckSpace(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
            }
            catch (Exception)
            {
            }
        }
        private bool IsValidTextNumber(string p)
        {
            try
            {
                return Regex.Match(p, "^[0-9]*$").Success;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
