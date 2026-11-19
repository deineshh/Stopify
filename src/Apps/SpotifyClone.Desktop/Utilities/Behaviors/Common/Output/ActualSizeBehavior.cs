using System.Windows;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Output;

public static class ActualSizeBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty ActualWidthProperty =
            DependencyProperty.RegisterAttached(
                "ActualWidth",
                typeof(double?),
                typeof(ActualSizeBehavior),
                new PropertyMetadata(null, OnActualWidthChanged));

    #endregion

    #region Getters/Setters


    public static double GetActualWidth(UIElement element) =>
        (double)element.GetValue(ActualWidthProperty);
    public static void SetActualWidth(UIElement element, double value) =>
        element.SetValue(ActualWidthProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnActualWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if (e.NewValue is not null)
        {
            element.SizeChanged += ExecuteSetActualWidth;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.SizeChanged -= ExecuteSetActualWidth;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void ExecuteSetActualWidth(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        SetActualWidth(element, element.ActualWidth);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.SizeChanged -= ExecuteSetActualWidth;
        element.Unloaded -= DetachEvents;
    }

    #endregion
}
