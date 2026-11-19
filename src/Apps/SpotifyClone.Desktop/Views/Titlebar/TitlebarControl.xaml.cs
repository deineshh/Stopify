using SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Titlebar;

public partial class TitlebarControl : UserControl
{
    public TitlebarControl()
    {
        InitializeComponent();

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.FriendActivityBtnWidthProperty, new Binding()
        {
            Source = FriendActivityBtn,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.NewsBtnWidthProperty, new Binding()
        {
            Source = WhatsNewBtn,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchBarWidthProperty, new Binding()
        {
            Source = SearchBar,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchBtnBorderRadiusProperty, new Binding()
        {
            Source = SearchBtnBorder,
            Path = new PropertyPath("CornerRadius"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchbarBrowseWidthProperty, new Binding()
        {
            Source = SearchbarBrowse,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchbarInputProperty, new Binding()
        {
            Source = SearchbarBox,
            Path = new PropertyPath("Text"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchbarInputWidthProperty, new Binding()
        {
            Source = SearchbarBox,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchbarLineWidthProperty, new Binding()
        {
            Source = SearchbarLine,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });

        BindingOperations.SetBinding(this, TitlebarSizeBehavior.SearchbarTextWidthProperty, new Binding()
        {
            Source = SearchbarTxt,
            Path = new PropertyPath("Width"),
            Mode = BindingMode.TwoWay
        });
    }
}
