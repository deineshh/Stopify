using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class SearchButtonBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty IsClickedProperty =
        DependencyProperty.RegisterAttached(
            "IsClicked",
            typeof(bool),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SearchBtnBorderProperty =
        DependencyProperty.RegisterAttached(
            "SearchBtnBorder",
            typeof(Border),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBarProperty =
        DependencyProperty.RegisterAttached(
            "SearchBar",
            typeof(Border),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBtnTextProperty =
        DependencyProperty.RegisterAttached(
            "SearchBtnText",
            typeof(TextBlock),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBarActualWidthProperty =
        DependencyProperty.RegisterAttached(
            "SearchBarActualWidth",
            typeof(double),
            typeof(SearchButtonBehavior),
            new PropertyMetadata(0.0));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static bool GetIsClicked(DependencyObject obj) =>
        (bool)obj.GetValue(IsClickedProperty);
    public static void SetIsClicked(DependencyObject obj, bool value) =>
        obj.SetValue(IsClickedProperty, value);

    public static Border GetSearchBtnBorder(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBtnBorderProperty);
    public static void SetSearchBtnBorder(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBtnBorderProperty, value);

    public static Border GetSearchBar(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBarProperty);
    public static void SetSearchBar(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBarProperty, value);

    public static TextBlock GetSearchBtnText(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchBtnTextProperty);
    public static void SetSearchBtnText(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchBtnTextProperty, value);

    public static double GetSearchBarActualWidth(DependencyObject obj) =>
        (double)obj.GetValue(SearchBarActualWidthProperty);
    public static void SetSearchBarActualWidth(DependencyObject obj, double value) =>
        obj.SetValue(SearchBarActualWidthProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Unloaded -= DetachEvents;

            SetIsClicked(element, false);
            SetSearchBarActualWidth(element, 0.0);
        }
    }

    #endregion

    #region Event Handlers

    private static void OnMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        TextBlock popupText = new()
        {
            Text = "Search",
            Foreground = Brushes.White,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
        };

        HoverPopupHelper.DisplayPopupTextBlock(element, PlacementMode.Bottom, popupText);

        Border searchBtnBorder = GetSearchBtnBorder(element);
        Border searchBar = GetSearchBar(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        ColorAnimations.AnimateBackground(searchBtnBorder, Color.FromRgb(54, 54, 53), .1);
        ColorAnimations.AnimateBackground(searchBar, Color.FromRgb(54, 54, 53), .1);
        ColorAnimations.AnimateForeground(searchBtnText, Colors.White, .2);

        if (GetSearchBarActualWidth(element) == 0)
        {
            ScaleAnimations.BeginScaleAnimation(element, 1.03, .2);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        HoverPopupHelper.HidePopup();

        Border searchBtnBorder = GetSearchBtnBorder(element);
        Border searchBar = GetSearchBar(element);
        TextBlock searchBtnText = GetSearchBtnText(element);

        ScaleAnimations.ResetScaleAnimation(element, .2);
        ColorAnimations.AnimateBackground(searchBtnBorder, Color.FromRgb(31, 31, 31), .1);
        ColorAnimations.AnimateBackground(searchBar, Color.FromRgb(31, 31, 31), .1);

        if (!GetIsClicked(element))
        {
            ColorAnimations.AnimateForeground(searchBtnText, Colors.DarkGray, .2);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= OnMouseEnter;
        element.MouseLeave -= OnMouseLeave;
        element.Unloaded -= DetachEvents;

        SetIsClicked(element, false);
        SetSearchBarActualWidth(element, 0.0);

        SetEnable(element, false);
    }

    #endregion
}
