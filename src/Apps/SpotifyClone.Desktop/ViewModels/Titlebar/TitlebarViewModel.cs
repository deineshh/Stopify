using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Commands.Base;
using SpotifyClone.Desktop.Utilities.Commands.Titlebar;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SpotifyClone.Desktop.ViewModels.Titlebar;

public class TitlebarViewModel : ViewModelBase
{
    private CornerRadius _searchbarBorderRadius = new(30);

    public bool IsOptionsMenuOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool CanNavigateBack
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public bool CanNavigateForward
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double TitlebarActualWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double FriendActivityBtnWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    public double NewsBtnWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    public double SearchBarWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public CornerRadius SearchBtnBorderRadius
    {
        get => _searchbarBorderRadius;
        set => SetProperty(ref _searchbarBorderRadius, value);
    }

    public double SearchbarInputWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double SearchbarBrowseWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string SearchbarInput
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public double SearchbarLineWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double SearchbarTextWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public double SearchBarActualWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public char AvatarPlaceholder
    {
        get;
        set => SetProperty(ref field, value);
    } = 'D';

    public string Username
    {
        get;
        set => SetProperty(ref field, value);
    } = "Dénes Trambola";

    public ObservableCollection<MenuItemViewModel> OptionsMenuItems { get; }

    public ICommand ToggleOptionsMenuCommand { get; }

    public ICommand NavigateBackCommand { get; }
    public ICommand NavigateForwardCommand { get; }

    public ICommand NavigateHomeCommand { get; }
    public ICommand ToggleSearchbarCommand { get; }
    public ICommand BrowseCommand { get; }

    public ICommand NavigateNewsCommand { get; }
    public ICommand ToggleFriendActivityCommand { get; }

    public TitlebarViewModel()
    {
        OptionsMenuItems = new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel("File", new RelayCommand(() => MessageBox.Show("File clicked"))),
            new MenuItemViewModel("Edit", new RelayCommand(() => MessageBox.Show("Edit clicked"))),
            new MenuItemViewModel("View", new RelayCommand(() => MessageBox.Show("View clicked"))),
            new MenuItemViewModel("Playback", new RelayCommand(() => MessageBox.Show("Playback clicked"))),
            new MenuItemViewModel("Help", new RelayCommand(() => MessageBox.Show("Help clicked")))
        };

        ToggleOptionsMenuCommand = new ToggleOptionsCommand(this);
        NavigateBackCommand = new NavigateBackCommand();
        NavigateForwardCommand = new NavigateForwardCommand();
        NavigateHomeCommand = new NavigateHomeCommand();
        ToggleSearchbarCommand = new ToggleSearchbarCommand(this);
        BrowseCommand = new TitlebarBrowseCommand();
        NavigateNewsCommand = new NavigateNewsCommand();
        ToggleFriendActivityCommand = new ToggleFriendActivityCommand();
    }
}
