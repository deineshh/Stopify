using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Animations;

public static class ForegroundColorAnimationBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableOnHoverProperty =
        DependencyProperty.RegisterAttached(
            "EnableOnHover",
            typeof(bool),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(false, OnEnableOnHoverChanged));

    public static readonly DependencyProperty EnableOnFocusProperty =
        DependencyProperty.RegisterAttached(
            "EnableOnFocus",
            typeof(bool),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(false, OnEnableOnFocusChanged));

    public static readonly DependencyProperty IsClickedProperty =
        DependencyProperty.RegisterAttached(
            "IsClicked",
            typeof(bool),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty InColorProperty =
        DependencyProperty.RegisterAttached(
            "InColor",
            typeof(Color),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty OutColorProperty =
        DependencyProperty.RegisterAttached(
            "OutColor",
            typeof(Color),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached(
            "Duration",
            typeof(double),
            typeof(ForegroundColorAnimationBehavior),
            new PropertyMetadata(0.0));

    #endregion

    #region Getters/Setters

    public static bool GetEnableOnHover(UIElement element) =>
        (bool)element.GetValue(EnableOnHoverProperty);
    public static void SetEnableOnHover(UIElement element, bool value) =>
        element.SetValue(EnableOnHoverProperty, value);

    public static bool GetEnableOnFocus(UIElement element) =>
        (bool)element.GetValue(EnableOnFocusProperty);
    public static void SetEnableOnFocus(UIElement element, bool value) =>
        element.SetValue(EnableOnFocusProperty, value);

    public static bool GetIsClicked(UIElement element) =>
        (bool)element.GetValue(IsClickedProperty);
    public static void SetIsClicked(UIElement element, bool value) =>
        element.SetValue(IsClickedProperty, value);

    public static Color GetInColor(UIElement element) =>
        (Color)element.GetValue(InColorProperty);
    public static void SetInColor(UIElement element, Color value) =>
        element.SetValue(InColorProperty, value);

    public static Color GetOutColor(UIElement element) =>
        (Color)element.GetValue(OutColorProperty);
    public static void SetOutColor(UIElement element, Color value) =>
        element.SetValue(OutColorProperty, value);

    public static double GetDuration(UIElement element) =>
        (double)element.GetValue(DurationProperty);
    public static void SetDuration(UIElement element, double value) =>
        element.SetValue(DurationProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += AnimateInIfNotClicked;
            element.MouseLeave += AnimateOutIfNotClicked;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= AnimateInIfNotClicked;
            element.MouseLeave -= AnimateOutIfNotClicked;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnEnableOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Control element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.GotFocus += AnimateIn;
            element.LostFocus += AnimateOut;
        }
        else
        {
            element.GotFocus -= AnimateIn;
            element.LostFocus -= AnimateOut;
        }
    }

    #endregion

    #region Event Handlers

    private static void AnimateInIfNotClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        if (!GetIsClicked(element))
        {
            ExecuteAnimateIn(element);
        }
    }

    private static void AnimateOutIfNotClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        if (!GetIsClicked(element))
        {
            ExecuteAnimateOut(element);
        }
    }

    private static void AnimateIn(object sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        ExecuteAnimateIn(element);
    }

    private static void AnimateOut(object sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        ExecuteAnimateOut(element);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        element.MouseEnter -= AnimateInIfNotClicked;
        element.MouseLeave -= AnimateOutIfNotClicked;
        element.Unloaded -= DetachEvents;

        SetEnableOnHover(element, false);
        SetEnableOnFocus(element, false);
    }

    #endregion

    #region Methods

    private static void ExecuteAnimateIn(Control element) =>
        ColorAnimations.AnimateForeground(element, GetInColor(element), GetDuration(element));

    private static void ExecuteAnimateOut(Control element) =>
        ColorAnimations.AnimateForeground(element, GetOutColor(element), GetDuration(element));

    #endregion
}
