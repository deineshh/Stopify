using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SpotifyClone.Desktop.Utilities.Animations;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class ToggleFilterBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(ToggleFilterBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.RegisterAttached(
            "GroupName",
            typeof(string),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TurnOffOnClick =
        DependencyProperty.RegisterAttached(
            "TurnOffOnClick",
            typeof(bool),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SelectedBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "SelectedBackgroundColor",
            typeof(Color),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(Colors.White));

    public static readonly DependencyProperty UnselectedBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "UnselectedBackgroundColor",
            typeof(Color),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(Color.FromRgb(42, 42, 42)));

    public static readonly DependencyProperty SelectedForegroundColorProperty =
        DependencyProperty.RegisterAttached(
            "SelectedForegroundColor",
            typeof(Color),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(Colors.Black));

    public static readonly DependencyProperty UnselectedForegroundColorProperty =
        DependencyProperty.RegisterAttached(
            "UnselectedForegroundColor",
            typeof(Color),
            typeof(ToggleFilterBehavior),
            new PropertyMetadata(Colors.White));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(UIElement element) =>
        (bool)element.GetValue(EnableProperty);
    public static void SetEnable(UIElement element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static bool GetIsSelected(UIElement element) =>
        (bool)element.GetValue(IsSelectedProperty);
    public static void SetIsSelected(UIElement element, bool value) =>
        element.SetValue(IsSelectedProperty, value);

    public static string GetGroupName(UIElement element) =>
        (string)element.GetValue(GroupNameProperty);
    public static void SetGroupName(UIElement element, string value) =>
        element.SetValue(GroupNameProperty, value);

    public static bool GetTurnOffOnClick(UIElement element) =>
        (bool)element.GetValue(TurnOffOnClick);
    public static void SetTurnOffOnClick(UIElement element, bool value) =>
        element.SetValue(TurnOffOnClick, value);

    public static Color GetSelectedBackgroundColor(UIElement element) =>
        (Color)element.GetValue(SelectedBackgroundColorProperty);
    public static void SetSelectedBackgroundColor(UIElement element, Color value) =>
        element.SetValue(SelectedBackgroundColorProperty, value);

    public static Color GetUnselectedBackgroundColor(UIElement element) =>
        (Color)element.GetValue(UnselectedBackgroundColorProperty);
    public static void SetUnselectedBackgroundColor(UIElement element, Color value) =>
        element.SetValue(UnselectedBackgroundColorProperty, value);

    public static Color GetSelectedForegroundColor(UIElement element) =>
        (Color)element.GetValue(SelectedForegroundColorProperty);
    public static void SetSelectedForegroundColor(UIElement element, Color value) =>
        element.SetValue(SelectedForegroundColorProperty, value);

    public static Color GetUnselectedForegroundColor(UIElement element) =>
        (Color)element.GetValue(UnselectedForegroundColorProperty);
    public static void SetUnselectedForegroundColor(UIElement element, Color value) =>
        element.SetValue(UnselectedForegroundColorProperty, value);


    #endregion

    #region Property Callbacks

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.PreviewMouseLeftButtonUp += ToggleFilter;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.PreviewMouseLeftButtonUp -= ToggleFilter;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element || !GetEnable(element) || VisualTreeHelper.GetChild(element, 0) is not Button button)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            ColorAnimations.AnimateBackground(element, GetSelectedBackgroundColor(element), 0.1);
            ColorAnimations.AnimateForeground(button, GetSelectedForegroundColor(element), 0.1);
        }
        else
        {
            ColorAnimations.AnimateBackground(element, GetUnselectedBackgroundColor(element), 0.1);
            ColorAnimations.AnimateForeground(button, GetUnselectedForegroundColor(element), 0.1);
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        if (!GetIsSelected(element))
        {
            ColorAnimations.AnimateBackground(element, Color.FromRgb(51, 51, 51), 0.1);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        if (!GetIsSelected(element))
        {
            ColorAnimations.AnimateBackground(element, GetUnselectedBackgroundColor(element), 0.1);
        }
    }

    private static void ToggleFilter(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        bool isCurrentlySelected = GetIsSelected(element);
        bool canTurnOffOnClick = GetTurnOffOnClick(element);

        if (isCurrentlySelected && canTurnOffOnClick)
        {
            SetIsSelected(element, false);
            return;
        }
        else if (!isCurrentlySelected)
        {
            SetIsSelected(element, true);
        }

        string group = GetGroupName(element);
        if (string.IsNullOrEmpty(group) || VisualTreeHelper.GetParent(element) is not Panel parent)
        {
            return;
        }

        foreach (UIElement child in parent.Children)
        {
            if (child is Border border && border != element
                && GetGroupName(border).Equals(group, StringComparison.Ordinal))
            {
                SetIsSelected(border, false);
            }
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.PreviewMouseLeftButtonUp -= ToggleFilter;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
