using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SpotifyClone.Desktop.ViewModels.Queue;

public class QueueViewModel : ViewModelBase
{
    private QueueItemViewModel _nowPlayingSong;
    private readonly UIState _uiState;

    // commented out for now
    //private readonly ObservableCollection<QueueItemViewModel> _recentlyPlayedSongs;

    public bool QueueCollapseState
    {
        get => _uiState.QueueCollapseState;
        set
        {
            if (_uiState.QueueCollapseState != value)
            {
                _uiState.QueueCollapseState = value;
                OnPropertyChanged();
            }
        }
    }

    public string PlaylistTitle
    {
        get;
        set => SetProperty(ref field, value);
    } = "Azahriah";

    public QueueItemViewModel NowPlayingSong
    {
        get => _nowPlayingSong;
        set => SetProperty(ref _nowPlayingSong, value);
    }

    public ObservableCollection<QueueItemViewModel> Songs { get; }

    public QueueViewModel(UIState uiState)
    {
        _nowPlayingSong = new QueueItemViewModel("introvertált dal", string.Empty);
        Songs = new ObservableCollection<QueueItemViewModel>()
        {
            new QueueItemViewModel("zene1", string.Empty),
            new QueueItemViewModel("zene2", string.Empty),
            new QueueItemViewModel("zene3", string.Empty),
            new QueueItemViewModel("zene4", string.Empty),
            new QueueItemViewModel("zene5", string.Empty),
            new QueueItemViewModel("zene6", string.Empty),
            new QueueItemViewModel("zene7", string.Empty),
            new QueueItemViewModel("zene8", string.Empty),
            new QueueItemViewModel("zene9", string.Empty),
            new QueueItemViewModel("zene10", string.Empty),
        };

        //_recentlyPlayedSongs = new ObservableCollection<QueueItemViewModel>()
        //{
        //    new QueueItemViewModel("zene1", string.Empty),
        //    new QueueItemViewModel("zene2", string.Empty),
        //    new QueueItemViewModel("zene3", string.Empty),
        //    new QueueItemViewModel("zene4", string.Empty),
        //    new QueueItemViewModel("zene5", string.Empty),
        //    new QueueItemViewModel("zene6", string.Empty),
        //    new QueueItemViewModel("zene7", string.Empty),
        //    new QueueItemViewModel("zene8", string.Empty),
        //    new QueueItemViewModel("zene9", string.Empty),
        //    new QueueItemViewModel("zene10", string.Empty),
        //};

        _uiState = uiState;
        _uiState.PropertyChanged += UIStatePropertyChanged;
    }

    private void UIStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_uiState.QueueCollapseState):
                OnPropertyChanged(nameof(QueueCollapseState));
                break;
        }
    }
}
