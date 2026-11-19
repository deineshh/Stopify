using SpotifyClone.Desktop.Utilities.Behaviors.Sidebar;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SpotifyClone.Desktop.Views.Sidebar;

public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();

        Binding visibilityBinding = new()
        {
            Source = YourLibraryText,
            Path = new PropertyPath("Visibility"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, SidebarSizeChangeBehavior.YourLibraryTextVisibilityProperty, visibilityBinding);

        Binding heightBinding = new()
        {
            Source = FilterBtns,
            Path = new PropertyPath("Height"),
            Mode = BindingMode.TwoWay
        };
        BindingOperations.SetBinding(
            this, SidebarSizeChangeBehavior.FilterBtnsHeightProperty, heightBinding);
    }
}
