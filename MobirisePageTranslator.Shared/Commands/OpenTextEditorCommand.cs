using MobirisePageTranslator.Shared.Data;
using MobirisePageTranslator.Shared.Editor;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace MobirisePageTranslator.Shared.Commands
{
    public sealed class OpenTextEditorCommand : FrameworkElement, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter is ContentCell;
        }

        public void Execute(object parameter)
        {
            var editor = (DataContext as MobiriseProjectViewModel)?.TextEditor;
            var popup = (DataContext as MobiriseProjectViewModel)?.TextEditorPopUp;
            var cellContent = parameter as ContentCell;
            if (cellContent != null && editor != null && popup != null)
            {
                var currentContent = cellContent.Content;
                editor.SetValue(TextEditor.TranslateProperty, cellContent);

                popup.IsOpen = true;
            }
        }
    }
}
