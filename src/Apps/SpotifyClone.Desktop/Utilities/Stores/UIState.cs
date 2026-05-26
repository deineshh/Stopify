using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpotifyClone.Desktop.Utilities.Stores;

public class UIState : INotifyPropertyChanged
{
    public bool SidebarCollapseState
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = true;

    public bool QueueCollapseState
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    } = true;

    public bool NowPlayingCollapseState
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
