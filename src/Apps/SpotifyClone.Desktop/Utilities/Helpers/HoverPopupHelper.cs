using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SpotifyClone.Desktop.Views.Main;

namespace SpotifyClone.Desktop.Utilities.Helpers;

public static class HoverPopupHelper
{
    private static readonly MainView _mainView = (MainView)Application.Current.MainWindow;

    private static readonly TextBlock _popupTextBlock = new()
    {
        Foreground = Brushes.White,
        Background = Brushes.Transparent,
        FontWeight = FontWeights.SemiBold,
        FontSize = 14
    };

    public static string PopupText
    {
        get => _popupTextBlock.Text;
        set => _popupTextBlock.Text = value;
    }

    public static void DisplayPopupTextBlock(FrameworkElement element, PlacementMode placement, params TextBlock[] texts)
    {
        Popup popup = _mainView.BtnPopup;

        popup.PlacementTarget = element;
        popup.Placement = placement;

        _mainView.PopupItem.PopupStackPanel.Children.Clear();
        foreach (TextBlock text in texts)
        {
            _mainView.PopupItem.PopupStackPanel.Children.Add(text);
        }

        popup.IsOpen = true;
    }

    public static void DisplayPopupText(FrameworkElement element, PlacementMode placement, params string[] texts)
    {
        Popup popup = _mainView.BtnPopup;

        popup.PlacementTarget = element;
        popup.Placement = placement;
        popup.IsOpen = true;

        _mainView.PopupItem.PopupStackPanel.Children.Clear();

        if (texts == null || texts.Length == 0)
        {
            _mainView.PopupItem.PopupStackPanel.Children.Add(_popupTextBlock);
            return;
        }

        foreach (string text in texts)
        {
            PopupText = text;
            _mainView.PopupItem.PopupStackPanel.Children.Add(_popupTextBlock);
        }
    }

    public static void HidePopup()
    {
        _mainView.BtnPopup.Visibility = Visibility.Collapsed;
        _mainView.BtnPopup.IsOpen = false;
    }
}
