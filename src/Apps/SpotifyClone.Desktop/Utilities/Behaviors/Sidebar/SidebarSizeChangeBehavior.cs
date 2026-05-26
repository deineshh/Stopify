using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Sidebar;

public static class SidebarSizeChangeBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SidebarSizeChangeBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty YourLibraryTextVisibilityProperty =
        DependencyProperty.RegisterAttached(
            "YourLibraryTextVisibility",
            typeof(Visibility),
            typeof(SidebarSizeChangeBehavior),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty FilterBtnsHeightProperty =
        DependencyProperty.RegisterAttached(
            "FilterBtnsHeight",
            typeof(double),
            typeof(SidebarSizeChangeBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Visibility GetYourLibraryTextVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(YourLibraryTextVisibilityProperty);
    public static void SetYourLibraryTextVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(YourLibraryTextVisibilityProperty, value);

    public static double GetFilterBtnsHeight(DependencyObject obj) =>
        (double)obj.GetValue(FilterBtnsHeightProperty);
    public static void SetFilterBtnsHeight(DependencyObject obj, double value) =>
        obj.SetValue(FilterBtnsHeightProperty, value);

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

        if (element.ActualWidth >= 280)
        {
            SetYourLibraryTextVisibility(element, Visibility.Visible);
            SetFilterBtnsHeight(element, double.NaN);
        }
        else
        {
            SetYourLibraryTextVisibility(element, Visibility.Hidden);
            SetFilterBtnsHeight(element, 0);
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
