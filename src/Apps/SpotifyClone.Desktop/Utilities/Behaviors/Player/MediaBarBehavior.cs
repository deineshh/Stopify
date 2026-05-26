using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Player;

// IMPORTANT: this behavior works fine only with SliderBehavior
public static class MediaBarBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(MediaBarBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty TimerProperty =
        DependencyProperty.RegisterAttached(
            "Timer",
            typeof(DispatcherTimer),
            typeof(MediaBarBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.RegisterAttached(
            "CurrentTime",
            typeof(string),
            typeof(MediaBarBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MediaPlayerProperty =
        DependencyProperty.RegisterAttached(
            "MediaPlayer",
            typeof(MediaPlayer),
            typeof(MediaBarBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static DispatcherTimer GetTimer(DependencyObject obj) =>
        (DispatcherTimer)obj.GetValue(TimerProperty);
    public static void SetTimer(DependencyObject obj, DispatcherTimer value) =>
        obj.SetValue(TimerProperty, value);

    public static string GetCurrentTime(DependencyObject obj) =>
        (string)obj.GetValue(CurrentTimeProperty);
    public static void SetCurrentTime(DependencyObject obj, string value) =>
        obj.SetValue(CurrentTimeProperty, value);

    public static MediaPlayer GetMediaPlayer(DependencyObject obj) =>
        (MediaPlayer)obj.GetValue(MediaPlayerProperty);
    public static void SetMediaPlayer(DependencyObject obj, MediaPlayer value) =>
        obj.SetValue(MediaPlayerProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Slider element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.PreviewMouseDown += OnMouseDown;
            element.PreviewMouseUp += OnMouseUp;
            element.PreviewMouseMove += OnMouseMove;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.PreviewMouseDown -= OnMouseDown;
            element.PreviewMouseUp -= OnMouseUp;
            element.PreviewMouseMove -= OnMouseMove;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        GetTimer(element).Stop();
        SetCurrentTimeBasedOnMediaBarValue(element);
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        GetMediaPlayer(element).Position = TimeSpan.FromSeconds(element.Value);
        GetTimer(element).Start();
        SetCurrentTimeBasedOnMediaPlayerPosition(element);
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Slider element || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        SetCurrentTimeBasedOnMediaBarValue(element);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        element.PreviewMouseDown -= OnMouseDown;
        element.PreviewMouseUp -= OnMouseUp;
        element.PreviewMouseMove -= OnMouseMove;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void SetCurrentTimeBasedOnMediaBarValue(Slider element) =>
        SetCurrentTime(element, TimeSpan.FromSeconds(element.Value).ToString(@"m\:ss"));

    private static void SetCurrentTimeBasedOnMediaPlayerPosition(Slider element) =>
        SetCurrentTime(element, GetMediaPlayer(element).Position.ToString(@"m\:ss"));

    #endregion
}
