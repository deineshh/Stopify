using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.NowPlaying;

public static class NowPlayingSizeChangeBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(NowPlayingSizeChangeBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty SaveTextMarginProperty =
        DependencyProperty.RegisterAttached(
            "SaveTextMargin",
            typeof(Thickness),
            typeof(NowPlayingSizeChangeBehavior),
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveTextFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "SaveTextFontSize",
            typeof(double),
            typeof(NowPlayingSizeChangeBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveButtonHeightProperty =
        DependencyProperty.RegisterAttached(
            "SaveButtonHeight",
            typeof(double),
            typeof(NowPlayingSizeChangeBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveBorderWidthProperty =
        DependencyProperty.RegisterAttached(
            "SaveBorderWidth",
            typeof(double),
            typeof(NowPlayingSizeChangeBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsSavedProperty =
        DependencyProperty.RegisterAttached(
            "IsSaved",
            typeof(bool),
            typeof(NowPlayingSizeChangeBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Thickness GetSaveTextMargin(DependencyObject obj) =>
        (Thickness)obj.GetValue(SaveTextMarginProperty);
    public static void SetSaveTextMargin(DependencyObject obj, Thickness value) =>
        obj.SetValue(SaveTextMarginProperty, value);

    public static double GetSaveTextFontSize(DependencyObject obj) =>
        (double)obj.GetValue(SaveTextFontSizeProperty);
    public static void SetSaveTextFontSize(DependencyObject obj, double value) =>
        obj.SetValue(SaveTextFontSizeProperty, value);

    public static double GetSaveButtonHeight(DependencyObject obj) =>
        (double)obj.GetValue(SaveButtonHeightProperty);
    public static void SetSaveButtonHeight(DependencyObject obj, double value) =>
        obj.SetValue(SaveButtonHeightProperty, value);

    public static double GetSaveBorderWidth(DependencyObject obj) =>
        (double)obj.GetValue(SaveBorderWidthProperty);
    public static void SetSaveBorderWidth(DependencyObject obj, double value) =>
        obj.SetValue(SaveBorderWidthProperty, value);

    public static bool GetIsSaved(DependencyObject obj) =>
        (bool)obj.GetValue(IsSavedProperty);
    public static void SetIsSaved(DependencyObject obj, bool value) =>
        obj.SetValue(IsSavedProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UserControl element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.SizeChanged += OnSizeChanged;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.SizeChanged -= OnSizeChanged;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        bool isSaved = GetIsSaved(element);

        if (element.ActualWidth >= 350)
        {
            if (isSaved)
            {
                SetSaveTextMargin(element, new(4, 3.5, 0, 0));
            }
            else
            {
                SetSaveTextMargin(element, new(2, .7, 0, 0));
            }

            SetSaveButtonHeight(element, 35);
            SetSaveBorderWidth(element, 21);
            SetSaveTextFontSize(element, 14);
        }
        else
        {
            if (isSaved)
            {
                SetSaveTextMargin(element, new(3, 3, 0, 0));
            }
            else
            {
                SetSaveTextMargin(element, new(1.5, .7, 0, 0));
            }

            SetSaveButtonHeight(element, 30);
            SetSaveBorderWidth(element, 16.5);
            SetSaveTextFontSize(element, 11);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        element.SizeChanged -= OnSizeChanged;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
