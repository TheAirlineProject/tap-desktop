using System.ComponentModel.Composition;
using System.Windows.Input;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using TaskDialogInterop;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.Infrastructure.Events;
using TheAirline.Infrastructure.Services;
using TheAirline.Models.General;

namespace TheAirline.ViewModels
{
    [Export]
    public class MainWindowViewModel : BindableBase
    {
        private readonly DelegateCommand<object> _closeCommand;
        private readonly IEventAggregator _events;
        private readonly ITaskDialogService _taskDialog;

        [ImportingConstructor]
        public MainWindowViewModel(IEventAggregator eventAggregator, ITaskDialogService taskDialog)
        {
            _events = eventAggregator;
            _taskDialog = taskDialog;
            _closeCommand = new DelegateCommand<object>(CloseGame);
        }

        public ICommand CloseCommand => _closeCommand;

        public void CloseGame(object ignored)
        {
            var opts = new TaskDialogOptions
            {
                Title = Translator.GetInstance().GetString("MessageBox", "1003"),
                MainInstruction = Translator.GetInstance().GetString("MessageBox", "1003", "message"),
                MainIcon = VistaTaskDialogIcon.Warning,
                CommonButtons = TaskDialogCommonButtons.YesNo
            };

            _taskDialog.ShowTaskDialog(opts, ExitResults);
        }

        private void ExitResults(TaskDialogResult obj)
        {
            if (obj.Result == TaskDialogSimpleResult.Yes)
            {
                _events.GetEvent<CloseGameEvent>().Publish(null);
            }
        }
    }
}