using SpotifyClone.Desktop.Utilities.Commands.Base;
using SpotifyClone.Desktop.ViewModels.Titlebar;
using System.Windows;

namespace SpotifyClone.Desktop.Utilities.Commands.Titlebar;

public class ToggleSearchbarCommand : CommandBase
{
    private readonly TitlebarViewModel _viewModel;

    public override void Execute(object? parameter)
    {
        if (_viewModel.SearchBarWidth == 0)
        {
            //_mainWindow.MainFrame.Navigate(new SearchView.SearchView());

            _viewModel.SearchbarTextWidth = double.NaN;
            _viewModel.SearchbarInputWidth = double.NaN;
            _viewModel.SearchbarLineWidth = double.NaN;
            _viewModel.SearchbarBrowseWidth = double.NaN;
            _viewModel.SearchBarWidth = double.NaN;
            _viewModel.SearchBtnBorderRadius = new CornerRadius(23, 0, 0, 23);

            if (_viewModel.TitlebarActualWidth < 850)
            {
                _viewModel.NewsBtnWidth = 0;
                _viewModel.FriendActivityBtnWidth = 0;
            }
            else
            {
                _viewModel.NewsBtnWidth = double.NaN;
                _viewModel.FriendActivityBtnWidth = double.NaN;
            }

            //ScaleAnimations.ResetScaleAnimation(SearchBtn, .2);
        }
        else
        {
            _viewModel.SearchbarTextWidth = 0;
            _viewModel.SearchbarInputWidth = 0;
            _viewModel.SearchbarLineWidth = 0;
            _viewModel.SearchbarBrowseWidth = 0;
            _viewModel.SearchBarWidth = 0;
            _viewModel.SearchBtnBorderRadius = new CornerRadius(30);
            _viewModel.SearchbarInput = string.Empty;

            _viewModel.NewsBtnWidth = double.NaN;
            _viewModel.FriendActivityBtnWidth = double.NaN;

            //ScaleAnimations.BeginScaleAnimation(SearchBtn, 1.03, .2);
        }
    }

    public ToggleSearchbarCommand(TitlebarViewModel titlebarViewModel)
        => _viewModel = titlebarViewModel;
}
