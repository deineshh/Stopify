namespace SpotifyClone.Desktop.Utilities.Commands.Base;

public class RelayCommand : RelayCommand<object>
{
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : base(_ => execute(), _ => canExecute?.Invoke() ?? true)
    {
    }
}