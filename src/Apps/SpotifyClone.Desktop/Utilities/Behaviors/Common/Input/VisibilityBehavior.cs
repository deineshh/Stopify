using System.Windows;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Input;

public static class VisibilityBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableOnHoverProperty = DependencyProperty.RegisterAttached(
        "EnableOnHover",
        typeof(bool),
        typeof(VisibilityBehavior),
        new PropertyMetadata(false, OnEnableOnHoverChanged));

    public static readonly DependencyProperty VisibilityOnMouseEnterProperty = DependencyProperty.RegisterAttached(
        "VisibilityOnMouseEnter",
        typeof(Visibility),
        typeof(VisibilityBehavior),
        new PropertyMetadata(null, OnEnableOnHoverChanged));

    public static readonly DependencyProperty VisibilityOnMouseLeaveProperty = DependencyProperty.RegisterAttached(
        "VisibilityOnMouseLeave",
        typeof(Visibility),
        typeof(VisibilityBehavior),
        new PropertyMetadata(null, OnEnableOnHoverChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnableOnHover(DependencyObject obj) =>
        (bool)obj.GetValue(EnableOnHoverProperty);
    public static void SetEnableOnHover(DependencyObject obj, bool value) =>
        obj.SetValue(EnableOnHoverProperty, value);

    public static Visibility GetVisibilityOnMouseEnter(DependencyObject obj) =>
        (Visibility)obj.GetValue(VisibilityOnMouseEnterProperty);
    public static void SetVisibilityOnMouseEnter(DependencyObject obj, Visibility value) =>
        obj.SetValue(VisibilityOnMouseEnterProperty, value);

    public static Visibility GetVisibilityOnMouseLeave(DependencyObject obj) =>
        (Visibility)obj.GetValue(VisibilityOnMouseLeaveProperty);
    public static void SetVisibilityOnMouseLeave(DependencyObject obj, Visibility value) =>
        obj.SetValue(VisibilityOnMouseLeaveProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += SetVisibility;
            element.MouseLeave += ResetVisibility;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= SetVisibility;
            element.MouseLeave -= ResetVisibility;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void SetVisibility(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.Visibility = GetVisibilityOnMouseEnter(element);
    }

    private static void ResetVisibility(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.Visibility = GetVisibilityOnMouseLeave(element);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MouseEnter -= SetVisibility;
        element.MouseLeave -= ResetVisibility;
        element.Unloaded -= DetachEvents;

        SetEnableOnHover(element, false);
    }

    #endregion
}
