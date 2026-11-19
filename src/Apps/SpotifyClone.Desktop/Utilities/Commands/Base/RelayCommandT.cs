using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Commands.Base;

public class RelayCommand<T>(
    Action<T> execute, Func<T, bool>? canExecute = null)
    : ICommand
{
    private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<T, bool> _canExecute = canExecute ?? (_ => true);

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute((T)parameter!);

    public void Execute(object? parameter) => _execute((T)parameter!);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
