using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace MobirisePageTranslator.Shared.Commands
{
    public sealed class RelayCommand : FrameworkElement, ICommand
    {
        public bool CanDoThis
        {
            get { return (bool)GetValue(CanDoThisProperty); }
            set { SetValue(CanDoThisProperty, value); }
        }

        public static readonly DependencyProperty CanDoThisProperty =
            DependencyProperty.Register(
                nameof(CanDoThis), 
                typeof(bool), 
                typeof(RelayCommand), 
                new PropertyMetadata(false, new PropertyChangedCallback((obj, args) =>
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public Func<object, bool> CanDoThisWithThat
        {
            get { return (Func<object, bool>)GetValue(CanDoThisWithThatProperty); }
            set { SetValue(CanDoThisWithThatProperty, value); }
        }

        public static readonly DependencyProperty CanDoThisWithThatProperty =
            DependencyProperty.Register(
                nameof(CanDoThisWithThat),
                typeof(Func<object, bool>),
                typeof(RelayCommand),
                new PropertyMetadata(new Func<bool>(() => true), new PropertyChangedCallback((obj, args) => 
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue && args.NewValue != null)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public Action<object> Dothis
        {
            get { return (Action<object>)GetValue(DothisProperty); }
            set { SetValue(DothisProperty, value); }
        }

        public static readonly DependencyProperty DothisProperty =
            DependencyProperty.Register(
                nameof(Dothis), 
                typeof(Action<object>), 
                typeof(RelayCommand), 
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) =>
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return Dothis != null && CanDoThis && CanDoThisWithThat.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            Dothis?.Invoke(parameter);
        }
    }
}
