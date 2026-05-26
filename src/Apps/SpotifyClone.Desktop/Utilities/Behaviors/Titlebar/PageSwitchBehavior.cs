using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Behaviors.Common.Animations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Titlebar;

public static class PageSwitchBehavior
{
    #region Properties

    private static Button HomeBtn { get; set; } = null!;
    private static Button SearchBtn { get; set; } = null!;
    private static TextBlock SearchIcon { get; set; } = null!;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(PageSwitchBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty HomeBtnProperty =
        DependencyProperty.RegisterAttached(
            "HomeBtn",
            typeof(Button),
            typeof(PageSwitchBehavior),
            new PropertyMetadata(null, OnHomeBtnChanged));

    public static readonly DependencyProperty SearchBtnProperty =
        DependencyProperty.RegisterAttached(
            "SearchBtn",
            typeof(Button),
            typeof(PageSwitchBehavior),
            new PropertyMetadata(null, OnSearchBtnChanged));

    public static readonly DependencyProperty SearchIconProperty =
        DependencyProperty.RegisterAttached(
            "SearchIcon",
            typeof(TextBlock),
            typeof(PageSwitchBehavior),
            new PropertyMetadata(null, OnSearchIconChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static Button GetHomeBtn(DependencyObject obj) =>
        (Button)obj.GetValue(HomeBtnProperty);
    public static void SetHomeBtn(DependencyObject obj, Button value) =>
        obj.SetValue(HomeBtnProperty, value);

    public static Button GetSearchBtn(DependencyObject obj) =>
        (Button)obj.GetValue(SearchBtnProperty);
    public static void SetSearchBtn(DependencyObject obj, Button value) =>
        obj.SetValue(SearchBtnProperty, value);

    public static TextBlock GetSearchIcon(DependencyObject obj) =>
        (TextBlock)obj.GetValue(SearchIconProperty);
    public static void SetSearchIcon(DependencyObject obj, TextBlock value) =>
        obj.SetValue(SearchIconProperty, value);

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
            element.Click += SwitchPage;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.Click -= SwitchPage;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnHomeBtnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Button element)
        {
            HomeBtn = element;
        }
    }

    private static void OnSearchBtnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Button element)
        {
            SearchBtn = element;
        }
    }

    private static void OnSearchIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock element)
        {
            SearchIcon = element;
        }
    }

    #endregion

    #region Event Handlers

    private static void SwitchPage(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (element == HomeBtn)
        {
            if (!GetEnable(element))
            {
                return;
            }

            ForegroundColorAnimationBehavior.SetIsClicked(element, true);
            ApplyForegroundAnimation(element, Colors.White);

            SearchButtonBehavior.SetIsClicked(SearchBtn, false);
            ApplyForegroundAnimation(SearchIcon, Colors.DarkGray);
        }
        else if (element == SearchBtn)
        {
            if (!GetEnable(element))
            {
                return;
            }

            SearchButtonBehavior.SetIsClicked(element, true);
            ApplyForegroundAnimation(SearchIcon, Colors.White);

            ForegroundColorAnimationBehavior.SetIsClicked(HomeBtn, false);
            ApplyForegroundAnimation(HomeBtn, Colors.DarkGray);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.Click -= SwitchPage;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void ApplyForegroundAnimation(Button element, Color targetColor) =>
        ColorAnimations.AnimateForeground(element, targetColor, 0.2);

    private static void ApplyForegroundAnimation(TextBlock element, Color targetColor) =>
        ColorAnimations.AnimateForeground(element, targetColor, 0.2);

    #endregion
}
