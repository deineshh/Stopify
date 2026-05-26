using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Stores;
using System.ComponentModel;

namespace SpotifyClone.Desktop.ViewModels.Sidebar;

public class SidebarItemViewModel : ViewModelBase
{
    private readonly UIState _uiState;

    public int PlaylistSongQuantity
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsPlaying
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool SidebarCollapseState
    {
        get => _uiState.SidebarCollapseState;
        set
        {
            if (_uiState.SidebarCollapseState != value)
            {
                _uiState.SidebarCollapseState = value;
                OnPropertyChanged(nameof(SidebarCollapseState));
            }
        }
    }

    public string PlaylistImagePath
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string PlaylistAuthor
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string PlaylistTitle
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string PlaylistType
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public SidebarItemViewModel(
        UIState uiState,
        string playlistTitle,
        string playlistType,
        string playlistImagePath,
        string? playlistAuthor = null,
        int playlistSongQuantity = default)
    {
        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        PlaylistTitle = playlistTitle;
        PlaylistType = playlistType;
        PlaylistImagePath = playlistImagePath;
        PlaylistAuthor = playlistAuthor ?? string.Empty;
        PlaylistSongQuantity = playlistSongQuantity;
    }

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(nameof(SidebarCollapseState));
}
