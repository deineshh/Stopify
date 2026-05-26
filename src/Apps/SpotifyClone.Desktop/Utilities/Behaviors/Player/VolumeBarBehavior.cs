using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Player;

// IMPORTANT: this behavior works only with SliderBehavior
public static class VolumeBarBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(VolumeBarBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty VolumeValueProperty =
        DependencyProperty.RegisterAttached(
            "VolumeValue",
            typeof(double),
            typeof(VolumeBarBehavior),
            new FrameworkPropertyMetadata((double)0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVolumeValueChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static double GetVolumeValue(DependencyObject obj) =>
        (double)obj.GetValue(VolumeValueProperty);
    public static void SetVolumeValue(DependencyObject obj, double value) =>
        obj.SetValue(VolumeValueProperty, value);

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
            element.ValueChanged += OnValueChanged;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.ValueChanged -= OnValueChanged;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnVolumeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Slider element)
        {
            return;
        }

        element.Value = (double)e.NewValue;
    }

    #endregion

    #region Event Handlers

    private static void OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        if (!GetEnable(element) && e.NewValue != e.OldValue)
        {
            UpdateVolumeValue(element);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Slider element)
        {
            return;
        }

        element.ValueChanged -= OnValueChanged;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void UpdateVolumeValue(Slider element)
        => SetVolumeValue(element, element.Value);

    #endregion
}
