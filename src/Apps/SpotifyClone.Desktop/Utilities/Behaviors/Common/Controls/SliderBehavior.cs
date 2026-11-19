using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class SliderBehavior
{
    #region Fields

    private static bool _isDragging;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SliderBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

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
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.PreviewMouseDown += OnMouseDown;
            element.PreviewMouseUp += OnMouseUp;
            element.PreviewMouseMove += OnMouseMove;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.PreviewMouseDown -= OnMouseDown;
            element.PreviewMouseUp -= OnMouseUp;
            element.PreviewMouseMove -= OnMouseMove;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        var track = (Track)element.Template.FindName("PART_Track", element);
        if (track is null)
        {
            return;
        }

        RepeatButton? decreaseButton = track.DecreaseRepeatButton;

        decreaseButton?.Background = new SolidColorBrush(Color.FromRgb(29, 185, 84));
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        var track = (Track)element.Template.FindName("PART_Track", element);
        if (track is null)
        {
            return;
        }

        RepeatButton? decreaseButton = track.DecreaseRepeatButton;
        decreaseButton?.Background = new SolidColorBrush(Colors.White);

        if (_isDragging)
        {
            _isDragging = false;
        }
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        _isDragging = true;
        UpdateSliderValue(element, e.GetPosition(element));
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Slider)
        {
            return;
        }

        _isDragging = false;
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        if (_isDragging)
        {
            UpdateSliderValue(element, e.GetPosition(element));
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.MouseDown -= OnMouseDown;
        element.MouseUp -= OnMouseUp;
        element.MouseMove -= OnMouseMove;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void UpdateSliderValue(Slider slider, Point mousePosition)
    {
        double relativePosition = mousePosition.X / slider.ActualWidth;
        double newValue = slider.Minimum + relativePosition * (slider.Maximum - slider.Minimum);
        slider.Value = newValue;
    }

    #endregion
}
