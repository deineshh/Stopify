using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class CloseWindowBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.RegisterAttached(
        "CloseOnClick",
        typeof(bool),
        typeof(CloseWindowBehavior),
        new PropertyMetadata(false, OnCloseOnClickChanged));

    #endregion

    #region Getters/Setters

    public static bool GetCloseOnClick(DependencyObject obj) =>
        (bool)obj.GetValue(CloseOnClickProperty);
    public static void SetCloseOnClick(DependencyObject obj, bool value) =>
        obj.SetValue(CloseOnClickProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnCloseOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.Click += Close;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.Click -= Close;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void Close(object sender, RoutedEventArgs e) =>
        Window.GetWindow((Button)sender).Close();

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.Click -= Close;
        element.Unloaded -= DetachEvents;

        SetCloseOnClick(element, false);
    }

    #endregion
}
