using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SpotifyClone.Desktop.Utilities.Animations;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class SearchBarBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SearchBarBehavior),
            new PropertyMetadata(false));

    public static readonly DependencyProperty SearchBoxProperty =
        DependencyProperty.RegisterAttached(
            "SearchBox",
            typeof(TextBox),
            typeof(SearchBarBehavior),
            new PropertyMetadata(null, OnSearchBoxChanged));

    public static readonly DependencyProperty SearchCloseButtonProperty =
        DependencyProperty.RegisterAttached(
            "SearchCloseButton",
            typeof(Button),
            typeof(SearchBarBehavior),
            new PropertyMetadata(null, OnSearchCloseButtonChanged));

    public static readonly DependencyProperty SearchButtonProperty =
        DependencyProperty.RegisterAttached(
            "SearchButton",
            typeof(Button),
            typeof(SearchBarBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBorderProperty =
        DependencyProperty.RegisterAttached(
            "SearchBorder",
            typeof(Border),
            typeof(SearchBarBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchTextBlockProperty =
        DependencyProperty.RegisterAttached(
            "SearchTextBlock",
            typeof(TextBlock),
            typeof(SearchBarBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SearchBarWidthProperty =
        DependencyProperty.RegisterAttached(
            "SearchBarWidth",
            typeof(double),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SearchBorderRadiusProperty =
        DependencyProperty.RegisterAttached(
            "SearchBorderRadius",
            typeof(CornerRadius),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SearchBarPlaceholderVisibilityProperty =
        DependencyProperty.RegisterAttached(
            "SearchBarPlaceholderVisibility",
            typeof(Visibility),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SearchCloseButtonWidthProperty =
        DependencyProperty.RegisterAttached(
            "SearchCloseButtonWidth",
            typeof(double),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsSearchingProperty =
        DependencyProperty.RegisterAttached(
            "IsSearching",
            typeof(bool),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSearchingChanged));

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.RegisterAttached(
            "SearchText",
            typeof(string),
            typeof(SearchBarBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    public static readonly DependencyProperty MouseLeaveSearchButtonBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "MouseLeaveSearchButtonBackgroundColor",
            typeof(Color),
            typeof(SearchBarBehavior),
            new PropertyMetadata(Color.FromRgb(18, 18, 18)));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static TextBox GetSearchBox(DependencyObject obj) =>
        (TextBox)obj.GetValue(SearchBoxProperty);
    public static void SetSearchBox(DependencyObject obj, TextBox value) =>
        obj.SetValue(SearchBoxProperty, value);

    public static Button GetSearchCloseButton(DependencyObject obj) =>
        (Button)obj.GetValue(SearchCloseButtonProperty);
    public static void SetSearchCloseButton(DependencyObject obj, Button value) =>
        obj.SetValue(SearchCloseButtonProperty, value);

    public static Button GetSearchButton(DependencyObject obj) =>
        (Button)obj.GetValue(SearchButtonProperty);
    public static void SetSearchButton(DependencyObject obj, Button value) =>
        obj.SetValue(SearchButtonProperty, value);

    public static Border GetSearchBorder(DependencyObject obj) =>
        (Border)obj.GetValue(SearchBorderProperty);
    public static void SetSearchBorder(DependencyObject obj, Border value) =>
        obj.SetValue(SearchBorderProperty, value);

    public static TextBlock GetSearchTextBlock(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchTextBlockProperty);
    public static void SetSearchTextBlock(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchTextBlockProperty, value);

    public static double GetSearchBarWidth(DependencyObject obj) =>
        (double)obj.GetValue(SearchBarWidthProperty);
    public static void SetSearchBarWidth(DependencyObject obj, double value) =>
        obj.SetValue(SearchBarWidthProperty, value);

    public static CornerRadius GetSearchBorderRadius(DependencyObject obj) =>
        (CornerRadius)obj.GetValue(SearchBorderRadiusProperty);
    public static void SetSearchBorderRadius(DependencyObject obj, CornerRadius value) =>
        obj.SetValue(SearchBorderRadiusProperty, value);

    public static Visibility GetSearchBarPlaceholderVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(SearchBarPlaceholderVisibilityProperty);
    public static void SetSearchBarPlaceholderVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(SearchBarPlaceholderVisibilityProperty, value);

    public static double GetSearchCloseButtonWidth(DependencyObject obj) =>
        (double)obj.GetValue(SearchCloseButtonWidthProperty);
    public static void SetSearchCloseButtonWidth(DependencyObject obj, double value) =>
        obj.SetValue(SearchCloseButtonWidthProperty, value);

    public static bool GetIsSearching(DependencyObject obj) =>
        (bool)obj.GetValue(IsSearchingProperty);
    public static void SetIsSearching(DependencyObject obj, bool value) =>
        obj.SetValue(IsSearchingProperty, value);

    public static string GetSearchText(DependencyObject obj) =>
        (string)obj.GetValue(SearchTextProperty);
    public static void SetSearchText(DependencyObject obj, string value) =>
        obj.SetValue(SearchTextProperty, value);

    public static Color GetMouseLeaveSearchButtonBackgroundColor(DependencyObject obj) =>
        (Color)obj.GetValue(MouseLeaveSearchButtonBackgroundColorProperty);
    public static void SetMouseLeaveSearchButtonBackgroundColor(DependencyObject obj, Color value) =>
        obj.SetValue(MouseLeaveSearchButtonBackgroundColorProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnSearchBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        if (e.OldValue is TextBox oldSearchBox)
        {
            oldSearchBox.GotFocus -= OnSearchBoxGotFocus;
            oldSearchBox.LostFocus -= OnSearchBoxLostFocus;
            oldSearchBox.PreviewLostKeyboardFocus -= OnSearchBoxPreviewLostKeyboardFocusHandler(element);
            oldSearchBox.Unloaded -= DetachSearchBoxEvents;
        }
        if (e.NewValue is TextBox newSearchBox && GetEnable(element))
        {
            newSearchBox.GotFocus += OnSearchBoxGotFocus;
            newSearchBox.LostFocus += OnSearchBoxLostFocus;
            newSearchBox.PreviewLostKeyboardFocus += OnSearchBoxPreviewLostKeyboardFocusHandler(element);
            newSearchBox.Unloaded += DetachSearchBoxEvents;
        }
    }

    private static void OnSearchCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        if (e.OldValue is Button oldButton)
        {
            oldButton.Click -= OnSearchCloseButtonClick;
            oldButton.Unloaded -= DetachCloseButtonEvents;
        }
        if (e.NewValue is Button newButton && GetEnable(element))
        {
            newButton.Click += OnSearchCloseButtonClick;
            newButton.Unloaded += DetachCloseButtonEvents;
        }
    }

    private static void OnIsSearchingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            GetSearchBox(element).Focus();
        }
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border element)
        {
            return;
        }

        string searchText = (string)e.NewValue;
        if (string.IsNullOrEmpty(searchText))
        {
            SetSearchBarPlaceholderVisibility(element, Visibility.Visible);
            SetSearchCloseButtonWidth(element, 0);
        }
        else
        {
            SetSearchBarPlaceholderVisibility(element, Visibility.Hidden);
            SetSearchCloseButtonWidth(element, double.NaN);
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSearchBoxGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox searchBox)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(searchBox)) is not Border element)
        {
            return;
        }

        SetSearchBorderRadius(element, new CornerRadius(5, 0, 0, 5));
        SetSearchBarWidth(element, double.NaN);
        SetSearchCloseButtonWidth(element, string.IsNullOrEmpty(GetSearchText(element)) ? 0 : double.NaN);
    }

    private static void OnSearchBoxLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox searchBox)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(searchBox)) is not Border element)
        {
            return;
        }

        SetSearchBorderRadius(element, new CornerRadius(30));
        SetSearchBarWidth(element, 0);
        SetSearchText(element, string.Empty);
        ColorAnimations.AnimateBackground(GetSearchBorder(element), GetMouseLeaveSearchButtonBackgroundColor(element), 0.1);
        ColorAnimations.AnimateForeground(GetSearchTextBlock(element), Colors.DarkGray, 0.2);
        SetIsSearching(element, false);
    }

    private static void OnSearchCloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button closeButton)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(closeButton)) is not Border element)
        {
            return;
        }

        SetSearchText(element, string.Empty);
    }

    private static void DetachSearchBoxEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox searchBox)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(searchBox)) is not Border element)
        {
            return;
        }

        searchBox.GotFocus -= OnSearchBoxGotFocus;
        searchBox.LostFocus -= OnSearchBoxLostFocus;
        searchBox.PreviewLostKeyboardFocus -= OnSearchBoxPreviewLostKeyboardFocusHandler(element);
        searchBox.Unloaded -= DetachSearchBoxEvents;

        SetEnable(element, false);
    }

    private static void DetachCloseButtonEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button closeButton)
        {
            return;
        }

        if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(closeButton)) is not Border element)
        {
            return;
        }

        closeButton.Click -= OnSearchCloseButtonClick;
        closeButton.Unloaded -= DetachCloseButtonEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static KeyboardFocusChangedEventHandler OnSearchBoxPreviewLostKeyboardFocusHandler(Border searchBar)
        => (sender, e) =>
        {
            if (e.NewFocus is not Button newFocus)
            {
                return;
            }
        
            if (newFocus == GetSearchButton(searchBar))
            {
                e.Handled = true;
            }
        };

    #endregion
}
