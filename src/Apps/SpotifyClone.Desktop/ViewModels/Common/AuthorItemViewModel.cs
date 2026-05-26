using SpotifyClone.Desktop.ViewModels.Base;
using SpotifyClone.Desktop.Utilities.Commands.Common;
using System.Windows.Input;

namespace SpotifyClone.Desktop.ViewModels.Common;

public class AuthorItemViewModel : ViewModelBase
{
    public string AuthorName
    {
        get;
        set => SetProperty(ref field, value);
    }

    public ICommand NavigateArtistCommand { get; }

    public AuthorItemViewModel(string authorName, NavigateArtistCommand navigateArtistCommand)
    {
        AuthorName = authorName;
        NavigateArtistCommand = navigateArtistCommand;
    }
}
