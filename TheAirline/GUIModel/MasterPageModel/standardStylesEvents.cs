namespace TheAirline.GUIModel.MasterPageModel
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class standardStylesEvents
    {
        #region Methods

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

        private void NumericTextBox_Input(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !this.IsValidTextNumber(e.Text);
            }
            catch (Exception)
            {
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        #endregion
    }
}