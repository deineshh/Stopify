using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Input;

public static class TextDecorationBehavior
{
    #region Enums

    public enum TextDecorationType
    {
        None,
        Underline,
        Overline,
        Strikethrough,
        Baseline,
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableOnHoverProperty = DependencyProperty.RegisterAttached(
        "EnableOnHover",
        typeof(bool),
        typeof(TextDecorationBehavior),
        new PropertyMetadata(false, OnEnableOnHoverChanged));

    public static readonly DependencyProperty TextDecorationOnMouseEnterProperty = DependencyProperty.RegisterAttached(
        "TextDecorationOnMouseEnter",
        typeof(TextDecorationType),
        typeof(TextDecorationBehavior),
        new PropertyMetadata(TextDecorationType.None));

    public static readonly DependencyProperty TextDecorationOnMouseLeaveProperty = DependencyProperty.RegisterAttached(
        "TextDecorationOnMouseLeave",
        typeof(TextDecorationType),
        typeof(TextDecorationBehavior),
        new PropertyMetadata(TextDecorationType.None));

    #endregion

    #region Getters/Setters

    public static bool GetEnableOnHover(DependencyObject obj) =>
        (bool)obj.GetValue(EnableOnHoverProperty);
    public static void SetEnableOnHover(DependencyObject obj, bool value) =>
        obj.SetValue(EnableOnHoverProperty, value);

    public static TextDecorationType GetTextDecorationOnMouseEnter(DependencyObject obj) =>
        (TextDecorationType)obj.GetValue(TextDecorationOnMouseEnterProperty);
    public static void SetTextDecorationOnMouseEnter(DependencyObject obj, TextDecorationType value) =>
        obj.SetValue(TextDecorationOnMouseEnterProperty, value);

    public static TextDecorationType GetTextDecorationOnMouseLeave(DependencyObject obj) =>
        (TextDecorationType)obj.GetValue(TextDecorationOnMouseLeaveProperty);
    public static void SetTextDecorationOnMouseLeave(DependencyObject obj, TextDecorationType value) =>
        obj.SetValue(TextDecorationOnMouseLeaveProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.MouseEnter += SetTextDecoration;
            element.MouseLeave += ResetTextDecoration;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= SetTextDecoration;
            element.MouseLeave -= ResetTextDecoration;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void SetTextDecoration(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        TextDecorationType type = GetTextDecorationOnMouseEnter(element);
        TextDecorationCollection? textDecorations = GetTextDecorationFromType(type);

        if (element.Content is TextBlock textBlock)
        {
            textBlock.TextDecorations = textDecorations;
        }
        else if (element.Content is string text)
        {
            var newTextBlock = new TextBlock { Text = text, TextDecorations = textDecorations };
            element.Content = newTextBlock;
        }
    }

    private static void ResetTextDecoration(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        TextDecorationType type = GetTextDecorationOnMouseLeave(element);
        TextDecorationCollection? textDecorations = GetTextDecorationFromType(type);

        if (element.Content is TextBlock textBlock)
        {
            textBlock.TextDecorations = textDecorations;
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.MouseEnter -= SetTextDecoration;
        element.MouseLeave -= ResetTextDecoration;
        element.Unloaded -= DetachEvents;

        SetEnableOnHover(element, false);
    }

    #endregion

    #region Methods

    private static TextDecorationCollection? GetTextDecorationFromType(TextDecorationType type)
        => type switch
        {
            TextDecorationType.Underline => TextDecorations.Underline,
            TextDecorationType.Overline => TextDecorations.OverLine,
            TextDecorationType.Strikethrough => TextDecorations.Strikethrough,
            TextDecorationType.Baseline => TextDecorations.Baseline,
            _ => null,
        };

    #endregion
}
