using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Queue.QueueItem;

public static class QueueItemBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(QueueItemBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty QueueItemBorderProperty =
        DependencyProperty.RegisterAttached(
            "QueueItemBorder",
            typeof(Border),
            typeof(QueueItemBehavior),
            new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Border GetQueueItemBorder(DependencyObject obj) =>
        (Border)obj.GetValue(QueueItemBorderProperty);
    public static void SetQueueItemBorder(DependencyObject obj, Border value) =>
        obj.SetValue(QueueItemBorderProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.GotFocus += OnGotFocus;
            element.LostFocus += OnLostFocus;
            element.Click += OnClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.GotFocus -= OnGotFocus;
            element.LostFocus -= OnLostFocus;
            element.Click -= OnClick;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!element.IsFocused)
        {
            ColorAnimations.AnimateBackground(GetQueueItemBorder(element), Color.FromRgb(31, 31, 31), .1);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!element.IsFocused)
        {
            ColorAnimations.AnimateBackground(GetQueueItemBorder(element), Color.FromRgb(18, 18, 18), .1);
        }
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetQueueItemBorder(element), Color.FromRgb(42, 42, 42), .05);
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetQueueItemBorder(element), Color.FromRgb(18, 18, 18), .05);
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.Focus();
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.GotFocus -= OnGotFocus;
        element.LostFocus -= OnLostFocus;
        element.Click -= OnClick;
    }

    #endregion
}
