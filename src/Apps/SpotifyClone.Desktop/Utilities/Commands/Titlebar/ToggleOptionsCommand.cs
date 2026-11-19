using SpotifyClone.Desktop.Utilities.Commands.Base;
using SpotifyClone.Desktop.ViewModels.Titlebar;

namespace SpotifyClone.Desktop.Utilities.Commands.Titlebar;

public class ToggleOptionsCommand : CommandBase
{
    private readonly TitlebarViewModel _titlebarViewModel;

    public override void Execute(object? parameter)
        => _titlebarViewModel.IsOptionsMenuOpen = !_titlebarViewModel.IsOptionsMenuOpen;

    public ToggleOptionsCommand(TitlebarViewModel titlebarViewModel)
        => _titlebarViewModel = titlebarViewModel;
}
