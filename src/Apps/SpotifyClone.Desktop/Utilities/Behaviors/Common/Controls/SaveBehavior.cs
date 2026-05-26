using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class SaveBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(SaveBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty IsSavedProperty = DependencyProperty.RegisterAttached(
        "IsSaved",
        typeof(bool),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSavedChanged));

    public static readonly DependencyProperty SaveToProperty = DependencyProperty.RegisterAttached(
        "SaveTo",
        typeof(string),
        typeof(SaveBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SaveBorderBrushProperty = DependencyProperty.RegisterAttached(
        "SaveBorderBrush",
        typeof(Brush),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveForegroundProperty = DependencyProperty.RegisterAttached(
        "SaveForeground",
        typeof(Brush),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveTextMarginProperty = DependencyProperty.RegisterAttached(
        "SaveTextMargin",
        typeof(Thickness),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveTextProperty = DependencyProperty.RegisterAttached(
        "SaveText",
        typeof(string),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveBorderBackgroundProperty = DependencyProperty.RegisterAttached(
        "SaveBorderBackground",
        typeof(Brush),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SaveBorderThicknessProperty = DependencyProperty.RegisterAttached(
        "SaveBorderThickness",
        typeof(Thickness),
        typeof(SaveBehavior),
        new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(UIElement element) =>
        (bool)element.GetValue(EnableProperty);
    public static void SetEnable(UIElement element, bool value) =>
        element.SetValue(EnableProperty, value);

    public static bool GetIsSaved(UIElement element) =>
        (bool)element.GetValue(IsSavedProperty);
    public static void SetIsSaved(UIElement element, bool value) =>
        element.SetValue(IsSavedProperty, value);

    public static string GetSaveTo(UIElement element) =>
        (string)element.GetValue(SaveToProperty);
    public static void SetSaveTo(UIElement element, string value) =>
        element.SetValue(SaveToProperty, value);

    public static Brush GetSaveBorderBrush(UIElement element) =>
        (Brush)element.GetValue(SaveBorderBrushProperty);
    public static void SetSaveBorderBrush(UIElement element, Brush value) =>
        element.SetValue(SaveBorderBrushProperty, value);

    public static Brush GetSaveForeground(UIElement element) =>
        (Brush)element.GetValue(SaveForegroundProperty);
    public static void SetSaveForeground(UIElement element, Brush value) =>
        element.SetValue(SaveForegroundProperty, value);

    public static Thickness GetSaveTextMargin(UIElement element) =>
        (Thickness)element.GetValue(SaveTextMarginProperty);
    public static void SetSaveTextMargin(UIElement element, Thickness value) =>
        element.SetValue(SaveTextMarginProperty, value);

    public static string GetSaveText(UIElement element) =>
        (string)element.GetValue(SaveTextProperty);
    public static void SetSaveText(UIElement element, string value) =>
        element.SetValue(SaveTextProperty, value);

    public static Brush GetSaveBorderBackground(UIElement element) =>
        (Brush)element.GetValue(SaveBorderBackgroundProperty);
    public static void SetSaveBorderBackground(UIElement element, Brush value) =>
        element.SetValue(SaveBorderBackgroundProperty, value);

    public static Thickness GetSaveBorderThickness(UIElement element) =>
        (Thickness)element.GetValue(SaveBorderThicknessProperty);
    public static void SetSaveBorderThickness(UIElement element, Thickness value) =>
        element.SetValue(SaveBorderThicknessProperty, value);

    #endregion

    #region Property Callbacks

    public static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Click += OnClick;
            element.Loaded += OnLoad;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Click -= OnClick;
            element.Loaded -= OnLoad;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnIsSavedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        UpdateUIBasedOnIsSavedState(element);
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        string saveTo = GetSaveTo(element);
        bool isSaved = GetIsSaved(element);
        HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top, isSaved
            ? $"Remove from {saveTo}"
            : $"Save to {saveTo}");

        if (!GetIsSaved(element))
        {
            SetSaveBorderBrush(element, Brushes.White);
            SetSaveForeground(element, Brushes.White);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        HoverPopupHelper.HidePopup();

        if (!GetIsSaved(element))
        {
            SetSaveBorderBrush(element, Brushes.DarkGray);
            SetSaveForeground(element, Brushes.DarkGray);
        }
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        bool isSaved = GetIsSaved(element);
        SetIsSaved(element, !isSaved);
        UpdateUIBasedOnIsSavedState(element);
    }

    private static void OnLoad(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        UpdateUIBasedOnIsSavedState(element);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Click -= OnClick;
        element.Loaded -= OnLoad;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void UpdateUIBasedOnIsSavedState(Button element)
    {
        bool isSaved = GetIsSaved(element);
        string saveTo = GetSaveTo(element);
        bool isHovering = element.IsMouseOver;

        if (isSaved)
        {
            SetSaveTextMargin(element, new Thickness(3, 3, 0, 0));
            SetSaveText(element, "\uf00c");
            SetSaveBorderBackground(element, new SolidColorBrush(Color.FromRgb(30, 215, 96)));
            SetSaveBorderThickness(element, new Thickness(0));
            SetSaveBorderBrush(element, Brushes.Transparent);
            SetSaveForeground(element, Brushes.Black);
            HoverPopupHelper.PopupText = $"Remove from {saveTo}";
        }
        else
        {
            HoverPopupHelper.PopupText = $"Save to {saveTo}";
            SetSaveTextMargin(element, new Thickness(1.5, .7, 0, 0));
            SetSaveText(element, "+");
            SetSaveBorderBackground(element, Brushes.Transparent);
            SetSaveBorderThickness(element, new Thickness(2));

            if (isHovering)
            {
                SetSaveBorderBrush(element, Brushes.White);
                SetSaveForeground(element, Brushes.White);
            }
            else
            {
                SetSaveBorderBrush(element, Brushes.DarkGray);
                SetSaveForeground(element, Brushes.DarkGray);
            }
        }
    }

    #endregion
}
