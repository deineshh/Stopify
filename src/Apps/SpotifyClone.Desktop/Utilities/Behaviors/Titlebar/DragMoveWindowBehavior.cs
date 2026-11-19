using System.Windows;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class DragMoveWindowBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableDragProperty =
        DependencyProperty.RegisterAttached(
            "EnableDrag",
            typeof(bool),
            typeof(DragMoveWindowBehavior),
            new PropertyMetadata(false, OnEnableDragChanged));

    #endregion

    #region Getters/Setters


    public static bool GetEnableDrag(UIElement element) =>
        (bool)element.GetValue(EnableDragProperty);
    public static void SetEnableDrag(UIElement element, bool value) =>
        element.SetValue(EnableDragProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseDown += DragMove;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseDown -= DragMove;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void DragMove(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            var window = Window.GetWindow(sender as DependencyObject);
            window?.DragMove();
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        element.MouseDown -= DragMove;
        element.Unloaded -= DetachEvents;

        SetEnableDrag(element, false);
    }

    #endregion
}
