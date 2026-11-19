using SpotifyClone.Desktop.ViewModels.Base;
using System.Windows.Input;

namespace SpotifyClone.Desktop.ViewModels.Titlebar;

public class MenuItemViewModel : ViewModelBase
{
    public string Header { get; set; }
    public ICommand Command { get; set; }

    public MenuItemViewModel(string header, ICommand command)
    {
        Header = header;
        Command = command;
    }
}
