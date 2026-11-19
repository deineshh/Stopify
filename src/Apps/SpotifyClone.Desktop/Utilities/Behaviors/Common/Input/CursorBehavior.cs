using System.Windows;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Input;

public static class CursorBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(CursorBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty HoverCursorProperty = DependencyProperty.RegisterAttached(
        "HoverCursor",
        typeof(Cursor),
        typeof(CursorBehavior),
        new PropertyMetadata(Cursors.Hand));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Cursor GetHoverCursor(DependencyObject obj) =>
        (Cursor)obj.GetValue(HoverCursorProperty);
    public static void SetHoverCursor(DependencyObject obj, Cursor value) =>
        obj.SetValue(HoverCursorProperty, value);

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
            element.MouseEnter += SetCursor;
            element.MouseLeave += ResetCursor;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= SetCursor;
            element.MouseLeave -= ResetCursor;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void SetCursor(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        Cursor cursor = GetHoverCursor(element);
        Mouse.OverrideCursor = cursor;
    }

    private static void ResetCursor(object sender, MouseEventArgs e) =>
        Mouse.OverrideCursor = Cursors.Arrow;

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MouseEnter -= SetCursor;
        element.MouseLeave -= ResetCursor;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
