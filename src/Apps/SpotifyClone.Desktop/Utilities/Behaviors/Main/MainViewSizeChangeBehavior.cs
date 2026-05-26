using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Main;

public static class MainViewSizeChangeBehavior
{
    #region Fields

    private const double MEDIUM_WINDOW_SIZE = 1100;
    private const double LARGE_WINDOW_SIZE = 1250;

    private const double SMALL_SIDEBAR_WIDTH = 81;
    private const double LARGE_SIDEBAR_WIDTH = 280;

    private const double SMALL_NOW_PLAYING_WIDTH = 281;
    private const double LARGE_NOW_PLAYING_WIDTH = 350;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(MainViewSizeChangeBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty NowPlayingCollapseStateProperty =
        DependencyProperty.RegisterAttached(
        "NowPlayingCollapseState",
        typeof(bool),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnNowPlayingCollapseStateChanged));

    public static readonly DependencyProperty SidebarCollapseStateProperty =
        DependencyProperty.RegisterAttached(
        "SidebarCollapseState",
        typeof(bool),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSidebarCollapseStateChanged));

    public static readonly DependencyProperty QueueCollapseStateProperty =
        DependencyProperty.RegisterAttached(
        "QueueCollapseState",
        typeof(bool),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnQueueCollapseStateChanged));

    public static readonly DependencyProperty QueueHeightProperty =
        DependencyProperty.RegisterAttached(
        "QueueHeight",
        typeof(double),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty QueueWidthProperty =
        DependencyProperty.RegisterAttached(
        "QueueWidth",
        typeof(double),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty NowPlayingActualHeightProperty =
        DependencyProperty.RegisterAttached(
        "NowPlayingHeight",
        typeof(double),
        typeof(MainViewSizeChangeBehavior),
        new PropertyMetadata((double)0));

    public static readonly DependencyProperty NowPlayingWidthProperty =
        DependencyProperty.RegisterAttached(
        "NowPlayingWidth",
        typeof(double),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SidebarWidthProperty =
        DependencyProperty.RegisterAttached(
        "SidebarWidth",
        typeof(double),
        typeof(MainViewSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty QueueBorderProperty =
        DependencyProperty.RegisterAttached(
        "QueueBorder",
        typeof(Border),
        typeof(MainViewSizeChangeBehavior),
        new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static bool GetNowPlayingCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(NowPlayingCollapseStateProperty);
    public static void SetNowPlayingCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(NowPlayingCollapseStateProperty, value);

    public static bool GetSidebarCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(SidebarCollapseStateProperty);
    public static void SetSidebarCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(SidebarCollapseStateProperty, value);

    public static bool GetQueueCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(QueueCollapseStateProperty);
    public static void SetQueueCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(QueueCollapseStateProperty, value);

    public static double GetQueueHeight(DependencyObject obj) =>
        (double)obj.GetValue(QueueHeightProperty);
    public static void SetQueueHeight(DependencyObject obj, double value) =>
        obj.SetValue(QueueHeightProperty, value);

    public static double GetQueueWidth(DependencyObject obj) =>
        (double)obj.GetValue(QueueWidthProperty);
    public static void SetQueueWidth(DependencyObject obj, double value) =>
        obj.SetValue(QueueWidthProperty, value);

    public static double GetNowPlayingActualHeight(DependencyObject obj) =>
        (double)obj.GetValue(NowPlayingActualHeightProperty);
    public static void SetNowPlayingActualHeight(DependencyObject obj, double value) =>
        obj.SetValue(NowPlayingActualHeightProperty, value);

    public static double GetNowPlayingWidth(DependencyObject obj) =>
        (double)obj.GetValue(NowPlayingWidthProperty);
    public static void SetNowPlayingWidth(DependencyObject obj, double value) =>
        obj.SetValue(NowPlayingWidthProperty, value);

    public static double GetSidebarWidth(DependencyObject obj) =>
        (double)obj.GetValue(SidebarWidthProperty);
    public static void SetSidebarWidth(DependencyObject obj, double value) =>
        obj.SetValue(SidebarWidthProperty, value);

    public static Border GetQueueBorder(DependencyObject obj) =>
        (Border)obj.GetValue(QueueBorderProperty);
    public static void SetQueueBorder(DependencyObject obj, Border value) =>
        obj.SetValue(QueueBorderProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.SizeChanged += OnSizeChanged;
            element.Loaded += OnLoaded;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.SizeChanged -= OnSizeChanged;
            element.Loaded -= OnLoaded;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnNowPlayingCollapseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        bool nowPlayingCollapseState = (bool)e.NewValue;
        bool sidebarCollapseState = GetSidebarCollapseState(window);
        bool queueCollapseState = GetQueueCollapseState(window);

        if (nowPlayingCollapseState)
        {
            SetNowPlayingWidth(window, 0);
            return;
        }
        else if (!queueCollapseState)
        {
            SetQueueCollapseState(window, true);
        }

        if (window.ActualWidth >= LARGE_WINDOW_SIZE)
        {
            SetNowPlayingWidth(window, LARGE_NOW_PLAYING_WIDTH);
        }
        else if (window.ActualWidth >= MEDIUM_WINDOW_SIZE)
        {
            SetNowPlayingWidth(window, sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH);
        }
        else
        {
            if (!sidebarCollapseState)
            {
                SetSidebarCollapseState(window, true);
            }
            else
            {
                SetNowPlayingWidth(window, SMALL_NOW_PLAYING_WIDTH);
            }
        }
    }

    private static void OnQueueCollapseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        Border queueBorder = GetQueueBorder(window);
        if (queueBorder is null)
        {
            return;
        }

        bool queueCollapseState = (bool)e.NewValue;
        bool sidebarCollapseState = GetSidebarCollapseState(window);
        bool nowPlayingCollapseState = GetNowPlayingCollapseState(window);

        if (queueCollapseState)
        {
            AnimateQueueHeightOut(window, queueBorder);
            return;
        }
        else if (!nowPlayingCollapseState)
        {
            SetNowPlayingCollapseState(window, true);
        }

        if (window.ActualWidth >= LARGE_WINDOW_SIZE)
        {
            SetQueueWidth(window, LARGE_NOW_PLAYING_WIDTH);
        }
        else if (window.ActualWidth >= MEDIUM_WINDOW_SIZE)
        {
            SetQueueWidth(window, sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH);
        }
        else
        {
            if (!sidebarCollapseState)
            {
                SetSidebarCollapseState(window, true);
            }
            else
            {
                SetQueueWidth(window, SMALL_NOW_PLAYING_WIDTH);
            }
        }

        AnimateQueueHeightIn(window, queueBorder);
    }

    private static void OnSidebarCollapseStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Window window)
        {
            return;
        }

        bool sidebarCollapseState = (bool)e.NewValue;
        bool nowPlayingCollapseState = GetNowPlayingCollapseState(window);
        bool queueCollapseState = GetQueueCollapseState(window);

        SetSidebarWidth(window, sidebarCollapseState ? SMALL_SIDEBAR_WIDTH : LARGE_SIDEBAR_WIDTH);

        if (window.ActualWidth >= LARGE_WINDOW_SIZE)
        {
            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, LARGE_NOW_PLAYING_WIDTH);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, LARGE_NOW_PLAYING_WIDTH);
            }
        }
        else if (window.ActualWidth >= MEDIUM_WINDOW_SIZE)
        {
            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH);
            }
        }
        else
        {
            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, sidebarCollapseState ? SMALL_NOW_PLAYING_WIDTH : 0);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, sidebarCollapseState ? SMALL_NOW_PLAYING_WIDTH : 0);
            }
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        bool nowPlayingCollapseState = GetNowPlayingCollapseState(window);
        bool sidebarCollapseState = GetSidebarCollapseState(window);
        bool queueCollapseState = GetQueueCollapseState(window);

        if (window.ActualWidth >= LARGE_WINDOW_SIZE)
        {
            if (!sidebarCollapseState)
            {
                SetSidebarWidth(window, LARGE_SIDEBAR_WIDTH);
            }

            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, LARGE_NOW_PLAYING_WIDTH);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, LARGE_NOW_PLAYING_WIDTH);
            }
        }
        else if (window.ActualWidth >= MEDIUM_WINDOW_SIZE)
        {
            double newNowPlayingWidth = sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH;

            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, newNowPlayingWidth);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, newNowPlayingWidth);
            }
        }
        else
        {
            if (!sidebarCollapseState && (!nowPlayingCollapseState || !queueCollapseState))
            {
                SetSidebarCollapseState(window, true);
                return;
            }

            if (!nowPlayingCollapseState)
            {
                SetNowPlayingWidth(window, SMALL_NOW_PLAYING_WIDTH);
            }

            if (!queueCollapseState)
            {
                SetQueueWidth(window, SMALL_NOW_PLAYING_WIDTH);
            }
        }

        if (!queueCollapseState)
        {
            SetQueueHeight(window, GetNowPlayingActualHeight(window) + 7);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        bool nowPlayingCollapseState = GetNowPlayingCollapseState(window);
        bool sidebarCollapseState = GetSidebarCollapseState(window);
        bool queueCollapseState = GetQueueCollapseState(window);

        if (!queueCollapseState)
        {
            SetQueueHeight(window, GetNowPlayingActualHeight(window) + 7);
        }

        if (window.ActualWidth >= LARGE_WINDOW_SIZE)
        {
            SetNowPlayingWidth(window, nowPlayingCollapseState ? 0 : LARGE_NOW_PLAYING_WIDTH);
            SetQueueWidth(window, queueCollapseState ? 0 : LARGE_NOW_PLAYING_WIDTH);
        }
        else if (window.ActualWidth >= MEDIUM_WINDOW_SIZE)
        {
            double newNowPlayingWidth = sidebarCollapseState ? LARGE_NOW_PLAYING_WIDTH : SMALL_NOW_PLAYING_WIDTH;
            SetNowPlayingWidth(window, nowPlayingCollapseState ? 0 : newNowPlayingWidth);
            SetQueueWidth(window, queueCollapseState ? 0 : newNowPlayingWidth);
        }
        else
        {
            SetNowPlayingWidth(window, nowPlayingCollapseState ? 0 : SMALL_NOW_PLAYING_WIDTH);
            SetQueueWidth(window, queueCollapseState ? 0 : SMALL_NOW_PLAYING_WIDTH);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Window element)
        {
            return;
        }

        element.SizeChanged -= OnSizeChanged;
        element.Loaded -= OnLoaded;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void AnimateQueueHeightIn(Window window, Border queueBorder)
    {
        double nowPlayingHeight = GetNowPlayingActualHeight(window);

        SetQueueHeight(window, nowPlayingHeight + 7);
        queueBorder.BeginAnimation(Border.HeightProperty, new DoubleAnimation
        {
            From = 0,
            To = nowPlayingHeight + 7,
            Duration = TimeSpan.FromSeconds(.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
        });
    }

    private static void AnimateQueueHeightOut(Window window, Border queueBorder)
    {
        double nowPlayingHeight = GetNowPlayingActualHeight(window);

        SetQueueHeight(window, 0);

        var animation = new DoubleAnimation
        {
            From = nowPlayingHeight + 7,
            To = 0,
            Duration = TimeSpan.FromSeconds(.3),
            EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
        };

        animation.Completed += (s, e) => SetQueueWidth(window, 0);

        queueBorder.BeginAnimation(Border.HeightProperty, animation);
    }

    #endregion
}
