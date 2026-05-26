using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Behaviors.Common.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Queue;

public static class QueueTabButtonBehavior
{
    #region Fields

    private static Button? _queueButton;
    private static Button? _recentButton;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(QueueTabButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(QueueTabButtonBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));

    public static readonly DependencyProperty QueueButtonProperty =
        DependencyProperty.RegisterAttached(
            "QueueButton",
            typeof(Button),
            typeof(QueueTabButtonBehavior),
            new PropertyMetadata(null, OnQueueButtonChanged));

    public static readonly DependencyProperty RecentButtonProperty =
        DependencyProperty.RegisterAttached(
            "RecentButton",
            typeof(Button),
            typeof(QueueTabButtonBehavior),
            new PropertyMetadata(null, OnRecentButtonChanged));

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

    public static Button GetQueueButton(UIElement element) =>
        (Button)element.GetValue(QueueButtonProperty);
    public static void SetQueueButton(UIElement element, Button value) =>
        element.SetValue(QueueButtonProperty, value);

    public static Button GetRecentButton(UIElement element) =>
        (Button)element.GetValue(RecentButtonProperty);
    public static void SetRecentButton(UIElement element, Button value) =>
        element.SetValue(RecentButtonProperty, value);

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
            element.Click += OnClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.Click -= OnClick;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        if (_queueButton is null || _recentButton is null)
        {
            return;
        }

        bool isSelected = (bool)e.NewValue;

        if (isSelected && isSelected != (bool)e.OldValue)
        {
            ColorAnimations.AnimateBorderBrush(element, Color.FromRgb(30, 215, 96), 0.1);
            ColorAnimations.AnimateForeground(element, Colors.White, 0.1);
            ForegroundColorAnimationBehavior.SetIsClicked(element, true);

            if (element == _queueButton)
            {
                SetIsSelected(_recentButton!, false);
            }
            else if (element == _recentButton)
            {
                SetIsSelected(_queueButton!, false);
            }
        }
        else if (!isSelected && isSelected != (bool)e.OldValue)
        {
            ColorAnimations.AnimateBorderBrush(element, Colors.Transparent, 0.1);
            ColorAnimations.AnimateForeground(element, Colors.DarkGray, 0.1);
            ForegroundColorAnimationBehavior.SetIsClicked(element, false);

            if (element == _queueButton)
            {
                SetIsSelected(_recentButton!, true);
            }
            else if (element == _recentButton)
            {
                SetIsSelected(_queueButton!, true);
            }
        }
    }

    private static void OnQueueButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is Button button)
        {
            _queueButton = button;
        }
    }

    private static void OnRecentButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is Button button)
        {
            _recentButton = button;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (!GetIsSelected(element))
        {
            SetIsSelected(element, true);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.Click -= OnClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
