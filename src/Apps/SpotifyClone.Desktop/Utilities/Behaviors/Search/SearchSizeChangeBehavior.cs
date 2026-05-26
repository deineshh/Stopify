using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Search;

public static class SearchSizeChangeBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(SearchSizeChangeBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty TotalColumnsProperty =
        DependencyProperty.RegisterAttached(
        "TotalColumns",
        typeof(int),
        typeof(SearchSizeChangeBehavior),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static int GetTotalColumns(DependencyObject obj) =>
        (int)obj.GetValue(TotalColumnsProperty);
    public static void SetTotalColumns(DependencyObject obj, int value) =>
        obj.SetValue(TotalColumnsProperty, value);

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

        double actualWidth = e.NewSize.Width;

        if (actualWidth < 730)
        {
            SetTotalColumns(element, 2);
        }
        else if (actualWidth < 1000)
        {
            SetTotalColumns(element, 3);
        }
        else
        {
            SetTotalColumns(element, 4);
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
