using SpotifyClone.Desktop.ViewModels.Base;
using System.IO;
using System.Windows;

namespace SpotifyClone.Desktop.ViewModels.Search;

public class SearchCategoryItemViewModel : ViewModelBase
{
    public string ImagePath
    {
        get;
        set
        {
            try
            {
                string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
                string imagePath = Path.Combine(projectDirectory, "Assets", "Images", "SearchPage", $"{value}.png");
                SetProperty(ref field, imagePath);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Search Category Image " + value + "does not exist in this directory!");
            }
        }
    } = string.Empty;

    public SearchCategoryItemViewModel(string imagePath)
        => ImagePath = imagePath;
}
