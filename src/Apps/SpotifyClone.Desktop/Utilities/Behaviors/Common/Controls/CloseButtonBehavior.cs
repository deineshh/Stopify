using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class CloseButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(CloseButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty CloseBorderProperty =
        DependencyProperty.RegisterAttached(
            "CloseBorder",
            typeof(Border),
            typeof(CloseButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CloseTextProperty =
        DependencyProperty.RegisterAttached(
            "CloseText",
            typeof(TextBlock),
            typeof(CloseButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsClosedProperty =
        DependencyProperty.RegisterAttached(
            "IsClosed",
            typeof(bool),
            typeof(CloseButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Border GetCloseBorder(DependencyObject obj) =>
        (Border)obj.GetValue(CloseBorderProperty);
    public static void SetCloseBorder(DependencyObject obj, Border value) =>
        obj.SetValue(CloseBorderProperty, value);

    public static TextBlock GetCloseText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(CloseTextProperty);
    public static void SetCloseText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(CloseTextProperty, value);

    public static bool GetIsClosed(DependencyObject obj) =>
        (bool)obj.GetValue(IsClosedProperty);
    public static void SetIsClosed(DependencyObject obj, bool value) =>
        obj.SetValue(IsClosedProperty, value);

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
            element.Click += OnClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnClick;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetCloseBorder(element), Color.FromRgb(31, 31, 31), .1);
        ColorAnimations.AnimateForeground(GetCloseText(element), Colors.LightGray, .1);
    }

    private static void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ColorAnimations.AnimateBackground(GetCloseBorder(element), Color.FromRgb(18, 18, 18), .1);
        ColorAnimations.AnimateForeground(GetCloseText(element), Colors.DarkGray, .1);
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        SetIsClosed(element, true);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Click -= OnClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
