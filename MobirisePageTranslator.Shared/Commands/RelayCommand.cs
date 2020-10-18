using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace MobirisePageTranslator.Shared.Commands
{
    public sealed class RelayCommand : DependencyObject, ICommand
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
                new PropertyMetadata(true, new PropertyChangedCallback((obj, args) =>
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
                new PropertyMetadata(new Func<object, bool>(obj => true), new PropertyChangedCallback((obj, args) => 
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue && args.NewValue != null)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public Func<bool> CanDoThatWithThis
        {
            get { return (Func<bool>)GetValue(CanDoThatWithThisProperty); }
            set { SetValue(CanDoThatWithThisProperty, value); }
        }

        public static readonly DependencyProperty CanDoThatWithThisProperty =
            DependencyProperty.Register(
                nameof(CanDoThatWithThis),
                typeof(Func<bool>),
                typeof(RelayCommand),
                new PropertyMetadata(new Func<bool>(() => true), new PropertyChangedCallback((obj, args) =>
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue && args.NewValue != null)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public Action<object> DoThis
        {
            get { return (Action<object>)GetValue(DoThisProperty); }
            set { SetValue(DoThisProperty, value); }
        }

        public static readonly DependencyProperty DoThisProperty =
            DependencyProperty.Register(
                nameof(DoThis),
                typeof(Action<object>), 
                typeof(RelayCommand), 
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) => 
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue && args.NewValue != null)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public Action DoThat
        {
            get { return (Action)GetValue(DoThatProperty); }
            set { SetValue(DoThatProperty, value); }
        }

        public static readonly DependencyProperty DoThatProperty =
            DependencyProperty.Register(
                nameof(DoThat),
                typeof(Action),
                typeof(RelayCommand),
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) =>
                {
                    var snd = (RelayCommand)obj;
                    if (args.NewValue != args.OldValue && args.NewValue != null)
                        snd.CanExecuteChanged?.Invoke(snd, EventArgs.Empty);
                })));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return (DoThat != null || DoThis != null) 
                && CanDoThis 
                && (CanDoThisWithThat.Invoke(parameter) || CanDoThatWithThis.Invoke());
        }

        public void Execute(object parameter)
        {
            DoThis?.Invoke(parameter);
            DoThat?.Invoke();
        }
    }
}
