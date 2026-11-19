using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class HoverPopupBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(HoverPopupBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PopupTextProperty =
        DependencyProperty.RegisterAttached(
            "PopupText",
            typeof(string),
            typeof(HoverPopupBehavior),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached(
            "Placement",
            typeof(PlacementMode),
            typeof(HoverPopupBehavior),
            new PropertyMetadata(PlacementMode.Top));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(UIElement element) =>
        (bool)element.GetValue(EnableProperty);
    public static void SetEnable(UIElement element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static string GetPopupText(UIElement element) =>
        (string)element.GetValue(PopupTextProperty);
    public static void SetPopupText(UIElement element, string value) =>
        element.SetValue(PopupTextProperty, value);

    public static PlacementMode GetPlacement(UIElement element) =>
        (PlacementMode)element.GetValue(PlacementProperty);
    public static void SetPlacement(UIElement element, PlacementMode value) =>
        element.SetValue(PlacementProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += DisplayHoverPopup;
            element.MouseLeave += HideHoverPopup;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= DisplayHoverPopup;
            element.MouseLeave -= HideHoverPopup;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void DisplayHoverPopup(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        HoverPopupHelper.DisplayPopupText(element, GetPlacement(element), GetPopupText(element));
    }

    private static void HideHoverPopup(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not FrameworkElement)
        {
            return;
        }

        HoverPopupHelper.HidePopup();
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MouseEnter -= DisplayHoverPopup;
        element.MouseLeave -= HideHoverPopup;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
