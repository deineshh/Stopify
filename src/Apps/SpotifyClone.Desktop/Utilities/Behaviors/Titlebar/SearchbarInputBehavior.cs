using SpotifyClone.Desktop.Utilities.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class SearchbarInputBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(SearchbarInputBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty SearchbarTextProperty = DependencyProperty.RegisterAttached(
        "SearchbarText",
        typeof(TextBlock),
        typeof(SearchbarInputBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBarProperty = DependencyProperty.RegisterAttached(
        "SearchBar",
        typeof(Border),
        typeof(SearchbarInputBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBtnBorderProperty = DependencyProperty.RegisterAttached(
        "SearchBtnBorder",
        typeof(Border),
        typeof(SearchbarInputBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBtnTextProperty = DependencyProperty.RegisterAttached(
        "SearchBtnText",
        typeof(TextBlock),
        typeof(SearchbarInputBehavior),
        new PropertyMetadata(null));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static TextBlock GetSearchbarText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchbarTextProperty);
    public static void SetSearchbarText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchbarTextProperty, value);

    public static Border GetSearchBar(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBarProperty);
    public static void SetSearchBar(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBarProperty, value);

    public static Border GetSearchBtnBorder(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBtnBorderProperty);
    public static void SetSearchBtnBorder(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBtnBorderProperty, value);

    public static TextBlock GetSearchBtnText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchBtnTextProperty);
    public static void SetSearchBtnText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchBtnTextProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.TextChanged += OnTextChanged;
            element.GotFocus += OnGotFocus;
            element.LostFocus += OnLostFocus;
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.TextChanged -= OnTextChanged;
            element.GotFocus -= OnGotFocus;
            element.LostFocus -= OnLostFocus;
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        GetSearchbarText(element).Foreground
            = string.IsNullOrEmpty(element.Text) ? Brushes.DarkGray : Brushes.Transparent;
    }

    private static void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        Border searchBar = GetSearchBar(element);
        Border searchBtnBorder = GetSearchBtnBorder(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        searchBar.BorderThickness = new Thickness(0, 2, 2, 2);
        searchBtnBorder.BorderThickness = new Thickness(2, 2, 0, 2);
        searchBtnBorder.Padding = new Thickness(0, 0, 2, 0);
        ColorAnimations.AnimateForeground(searchBtnText, Colors.White, .1);
    }

    private static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        Border searchBar = GetSearchBar(element);
        Border searchBtnBorder = GetSearchBtnBorder(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        searchBar.BorderThickness = new Thickness(0);
        searchBtnBorder.BorderThickness = new Thickness(0);
        searchBtnBorder.Padding = new Thickness(0);
        ColorAnimations.AnimateForeground(searchBtnText, Colors.DarkGray, .1);
        ColorAnimations.AnimateBackground(searchBtnBorder, Color.FromRgb(31, 31, 31), .1);
        ColorAnimations.AnimateBackground(searchBar, Color.FromRgb(31, 31, 31), .1);
    }

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        Border searchBar = GetSearchBar(element);
        Border searchBtnBorder = GetSearchBtnBorder(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        ColorAnimations.AnimateBackground(searchBtnBorder, Color.FromRgb(54, 54, 53), .1);
        ColorAnimations.AnimateBackground(searchBar, Color.FromRgb(54, 54, 53), .1);
        ColorAnimations.AnimateForeground(searchBtnText, Colors.White, .1);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        Border searchBar = GetSearchBar(element);
        Border searchBtnBorder = GetSearchBtnBorder(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        if (!element.IsFocused)
        {
            ColorAnimations.AnimateForeground(searchBtnText, Colors.DarkGray, .1);
            ColorAnimations.AnimateBackground(searchBtnBorder, Color.FromRgb(31, 31, 31), .1);
            ColorAnimations.AnimateBackground(searchBar, Color.FromRgb(31, 31, 31), .1);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox element)
        {
            return;
        }

        element.TextChanged -= OnTextChanged;
        element.GotFocus -= OnGotFocus;
        element.LostFocus -= OnLostFocus;
        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
