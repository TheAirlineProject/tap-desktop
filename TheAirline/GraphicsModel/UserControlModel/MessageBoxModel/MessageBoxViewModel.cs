using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace TheAirline.GraphicsModel.UserControlModel.MessageBoxModel
{
    public enum WPFMessageBoxResult
    {
        Yes,
        No,
        Ok,
        Cancel,
        Close
    }
    public enum WPFMessageBoxButtons
    {
        YesNo,
        Ok
    }
    public class MessageBoxViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string Title
        {
            get { return ___Title; }
            set
            {
                if (___Title != value)
                {
                    ___Title = value;
                    NotifyPropertyChange("Title");
                }
            }
        }

        public string Message
        {
            get { return ___Message; }
            set
            {
                if (___Message != value)
                {
                    ___Message = value;
                    NotifyPropertyChange("Message");
                }
            }
        }

        public Visibility YesNoVisibility
        {
            get { return ___YesNoVisibility; }
            set
            {
                if (___YesNoVisibility != value)
                {
                    ___YesNoVisibility = value;
                    NotifyPropertyChange("YesNoVisibility");
                }
            }
        }

        public Visibility CancelVisibility
        {
            get { return ___CancelVisibility; }
            set
            {
                if (___CancelVisibility != value)
                {
                    ___CancelVisibility = value;
                    NotifyPropertyChange("CancelVisibility");
                }
            }
        }

        public Visibility OkVisibility
        {
            get { return ___OKVisibility; }
            set
            {
                if (___OKVisibility != value)
                {
                    ___OKVisibility = value;
                    NotifyPropertyChange("OkVisibility");
                }
            }
        }

        public ICommand YesCommand
        {
            get
            {
                if (___YesCommand == null)
                    ___YesCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFMessageBoxResult.Yes;
                        ___View.Close();
                    });
                return ___YesCommand;
            }
        }

        public ICommand NoCommand
        {

            get
            {
                if (___NoCommand == null)
                    ___NoCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFMessageBoxResult.No;
                        ___View.Close();
                    });
                return ___NoCommand;
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (___OKCommand == null)
                    ___OKCommand = new DelegateCommand(() =>
                    {
                        ___View.Result = WPFMessageBoxResult.Ok;
                        ___View.Close();
                    });
                return ___OKCommand;
            }
        }






        public MessageBoxViewModel(WPFMessageBox view,
            string title, string message, WPFMessageBoxButtons buttons)
        {
            Title = title;
            Message = message;

            ___View = view;

            setButtonsVisibility(buttons);
        }

        public void NotifyPropertyChange(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        private void setButtonsVisibility(WPFMessageBoxButtons buttonOption)
        {
            switch (buttonOption)
            {
                case WPFMessageBoxButtons.YesNo:
                    OkVisibility = CancelVisibility = Visibility.Collapsed;
                    break;
                case WPFMessageBoxButtons.Ok:
                    YesNoVisibility = CancelVisibility = Visibility.Collapsed;
                    break;
                default:
                    OkVisibility = CancelVisibility = YesNoVisibility = Visibility.Collapsed;
                    break;
            }
        }



        Visibility ___YesNoVisibility;
        Visibility ___OKVisibility;
        Visibility ___CancelVisibility;

        ICommand ___YesCommand;
        ICommand ___NoCommand;
        ICommand ___OKCommand;




        string ___Title;
        string ___Message;


        WPFMessageBox ___View;

    }
}
