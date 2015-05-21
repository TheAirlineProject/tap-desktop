using System;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;
using TaskDialogInterop;
using TheAirline.Infrastructure.Services;
using TheAirline.Models.General;

namespace TheAirline.ViewModels.Game
{
    [Export]
    public class PageStartMenuViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ITaskDialogService _taskDialog;
        private Window _parent;

        [ImportingConstructor]
        public PageStartMenuViewModel(IRegionManager regionManager, ITaskDialogService taskDialog)
        {
            _regionManager = regionManager;
            _taskDialog = taskDialog;
            NavigateCommand = new DelegateCommand<Uri>(Navigate);
            ExitCommand = new DelegateCommand<Window>(ExitGame);
        }

        public DelegateCommand<Uri> NavigateCommand { get; }
        public DelegateCommand<Window> ExitCommand { get; }
        public Uri NewGameUri => new Uri("/PageStartGameData", UriKind.Relative);
        public Uri LoadGameUri => new Uri("/PageLoadGame", UriKind.Relative);
        public Uri SettingsUri => new Uri("/PageSettings", UriKind.Relative);
        public Uri CreditsUri => new Uri("/PageCredits", UriKind.Relative);

        private void Navigate(Uri view)
        {
            _regionManager.RequestNavigate("MainContentRegion", view);
        }

        private void ExitGame(Window window)
        {
            _parent = window;

            var opts = new TaskDialogOptions
            {
                Owner = window,
                Title = Translator.GetInstance().GetString("MessageBox", "1003"),
                MainInstruction = Translator.GetInstance().GetString("MessageBox", "1003", "message"),
                MainIcon = VistaTaskDialogIcon.Warning,
                CommonButtons = TaskDialogCommonButtons.YesNo
            };

            _taskDialog.ShowTaskDialog(opts, ExitResult);
        }

        private void ExitResult(TaskDialogResult results)
        {
            if (results.Result == TaskDialogSimpleResult.Yes)
            {
                _parent.Close();
            }
        }
    }
}