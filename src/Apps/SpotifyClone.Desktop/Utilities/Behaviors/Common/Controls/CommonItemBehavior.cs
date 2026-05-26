using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class CommonItemBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(CommonItemBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PlayButtonProperty =
        DependencyProperty.RegisterAttached(
            "PlayButton",
            typeof(Button),
            typeof(CommonItemBehavior),
            new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Button GetPlayButton(DependencyObject obj) =>
        (Button)obj.GetValue(PlayButtonProperty);
    public static void SetPlayButton(DependencyObject obj, Button value) =>
        obj.SetValue(PlayButtonProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Unloaded -= DetachEvents;
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

        Button playBtn = GetPlayButton(element);
        if (playBtn is null)
        {
            return;
        }

        var fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveUpAnimation = new DoubleAnimation
        {
            From = 20,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
        };

        playBtn.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        TranslateTransform transform = playBtn.RenderTransform as TranslateTransform ?? new TranslateTransform();
        playBtn.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.YProperty, moveUpAnimation);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        Button playBtn = GetPlayButton(element);
        if (playBtn is null)
        {
            return;
        }

        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
        };

        var moveDownAnimation = new DoubleAnimation
        {
            From = 0,
            To = 20,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
        };

        playBtn.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
        TranslateTransform transform = playBtn.RenderTransform as TranslateTransform ?? new TranslateTransform();
        playBtn.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.YProperty, moveDownAnimation);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Border element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
