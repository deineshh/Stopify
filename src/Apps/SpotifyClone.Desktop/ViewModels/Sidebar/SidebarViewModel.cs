using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpotifyClone.Desktop.ViewModels.Sidebar;

public class SidebarViewModel : ViewModelBase
{
    private readonly UIState _uiState;

    public bool IsPlaylistsFilter
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsArtistsFilter
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsSearching
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsExpanded
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
                OnPropertyChanged();
            }
        }
    }

    public double Width
    {
        get;
        set
        {
            SetProperty(ref field, value);
            IsExpanded = field >= 280;
        }
    }

    public string SearchText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public ObservableCollection<SidebarItemViewModel> Items { get; }

    public SidebarViewModel(UIState uiState)
    {
        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;

        Items = new ObservableCollection<SidebarItemViewModel>
        {
            new SidebarItemViewModel(_uiState, "Liked Songs", "Playlist", string.Empty, string.Empty, 80),
            new SidebarItemViewModel(_uiState, "Coding Music Programming Lofi Songs", "Playlist", string.Empty, "programmer"),
            new SidebarItemViewModel(_uiState, "Azahriah", "Artist", string.Empty),
            new SidebarItemViewModel(_uiState, "GYM PHONK 2025 AGGRESSIVE WORKOUT MUSIC", "Playlist", string.Empty, "Magic Records"),
            new SidebarItemViewModel(_uiState, "tiktok gym edits 2025 workout music", "Playlist", string.Empty, "Love Bedroom Pop"),
            new SidebarItemViewModel(_uiState, "VILE PHONK", "Playlist", string.Empty, "VILE MUSIC (IG: @vileplaylist)"),
            new SidebarItemViewModel(_uiState, "Kutyaknakplaylist", "Playlist", string.Empty, "KárojbossKrisztián"),
            new SidebarItemViewModel(_uiState, "MILLIONAIRE MODE Viral Tiktok Songs", "Playlist", string.Empty, "Sounds"),
            new SidebarItemViewModel(_uiState, "aesthetic gym posing", "Playlist", string.Empty, "_"),
            new SidebarItemViewModel(_uiState, "YAKTAK", "Artist", string.Empty),
        };
    }

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_uiState.SidebarCollapseState):
                OnPropertyChanged(nameof(SidebarCollapseState));
                break;
                // other state properties can be added here if needed
        }
    }
}
