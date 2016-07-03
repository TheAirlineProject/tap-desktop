using System;
using System.ComponentModel.Composition;

using TaskDialogInterop;

namespace TheAirline.Infrastructure.Services
{

    /// <summary>
    /// Service that handles displaying task dialogs.
    /// </summary>
    [InheritedExport]
    public interface ITaskDialogService
    {
        /// <summary>
        /// Shows a task dialog.
        /// </summary>
        /// <param name="options">A <see cref="T:TaskDialogOptions"/> config object.</param>
        /// <param name="callback">An optional callback method.</param>
        void ShowTaskDialog(TaskDialogOptions options, Action<TaskDialogResult> callback);
    }

    /// <summary>
    /// Service that handles displaying task dialogs.
    /// </summary>
    public class TaskDialogService : ITaskDialogService
    {
        /// <summary>
        /// Shows a task dialog.
        /// </summary>
        /// <param name="options">A <see cref="T:TaskDialogOptions"/> config object.</param>
        /// <param name="callback">An optional callback method.</param>
        public void ShowTaskDialog(TaskDialogOptions options, Action<TaskDialogResult> callback)
        {
            TaskDialogResult result = TaskDialog.Show(options);

            callback?.Invoke(result);
        }
    }
}