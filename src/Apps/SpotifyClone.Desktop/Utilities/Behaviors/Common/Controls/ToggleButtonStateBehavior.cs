using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SpotifyClone.Desktop.Utilities.Animations;
using SpotifyClone.Desktop.Utilities.Collections;
using SpotifyClone.Desktop.Utilities.Helpers;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Common.Controls;

public static class ToggleButtonStateBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(ToggleButtonStateBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty StateQuantityProperty =
        DependencyProperty.RegisterAttached(
            "StateQuantity",
            typeof(byte),
            typeof(ToggleButtonStateBehavior),
            new PropertyMetadata((byte)0));

    public static readonly DependencyProperty CurrentStateProperty =
        DependencyProperty.RegisterAttached(
            "CurrentState",
            typeof(byte),
            typeof(ToggleButtonStateBehavior),
            new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCurrentStateChanged));

    public static readonly DependencyProperty MouseEnterColorProperty =
        DependencyProperty.RegisterAttached(
            "MouseEnterColor",
            typeof(Color?),
            typeof(ToggleButtonStateBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MouseLeaveColorProperty =
        DependencyProperty.RegisterAttached(
            "MouseLeaveColor",
            typeof(Color?),
            typeof(ToggleButtonStateBehavior),
            new PropertyMetadata(null));

    public static readonly DependencyProperty StateInfosProperty =
        DependencyProperty.RegisterAttached(
            "StateInfos",
            typeof(ButtonStateCollection),
            typeof(ToggleButtonStateBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty DefaultHoverPopupTextProperty =
        DependencyProperty.RegisterAttached(
            "DefaultHoverPopupText",
            typeof(string),
            typeof(ToggleButtonStateBehavior),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StateDependentElementProperty =
    DependencyProperty.RegisterAttached(
        "StateDependentElement",
        typeof(FrameworkElement),
        typeof(ToggleButtonStateBehavior),
        new PropertyMetadata(null));

    public static readonly DependencyProperty StateDependentValueProperty =
    DependencyProperty.RegisterAttached(
        "StateDependentValue",
        typeof(object),
        typeof(ToggleButtonStateBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static byte GetStateQuantity(DependencyObject obj) =>
        (byte)obj.GetValue(StateQuantityProperty);
    public static void SetStateQuantity(DependencyObject obj, byte value) =>
        obj.SetValue(StateQuantityProperty, value);

    public static byte GetCurrentState(DependencyObject obj) =>
        (byte)obj.GetValue(CurrentStateProperty);
    public static void SetCurrentState(DependencyObject obj, byte value) =>
        obj.SetValue(CurrentStateProperty, value);

    public static Color? GetMouseEnterColor(DependencyObject obj) =>
        (Color?)obj.GetValue(MouseEnterColorProperty);
    public static void SetMouseEnterColor(DependencyObject obj, Color? value) =>
        obj.SetValue(MouseEnterColorProperty, value);

    public static Color? GetMouseLeaveColor(DependencyObject obj) =>
        (Color?)obj.GetValue(MouseLeaveColorProperty);
    public static void SetMouseLeaveColor(DependencyObject obj, Color? value) =>
        obj.SetValue(MouseLeaveColorProperty, value);

    public static ButtonStateCollection GetStateInfos(DependencyObject obj) =>
        (ButtonStateCollection)obj.GetValue(StateInfosProperty);
    public static void SetStateInfos(DependencyObject obj, ButtonStateCollection value) =>
        obj.SetValue(StateInfosProperty, value);

    public static string GetDefaultHoverPopupText(DependencyObject obj) =>
        (string)obj.GetValue(DefaultHoverPopupTextProperty);
    public static void SetDefaultHoverPopupText(DependencyObject obj, string value) =>
        obj.SetValue(DefaultHoverPopupTextProperty, value);

    public static FrameworkElement GetStateDependentElement(DependencyObject obj) =>
    (FrameworkElement)obj.GetValue(StateDependentElementProperty);
    public static void SetStateDependentElement(DependencyObject obj, FrameworkElement value) =>
        obj.SetValue(StateDependentElementProperty, value);

    public static object GetStateDependentValue(DependencyObject obj) =>
        obj.GetValue(StateDependentValueProperty);
    public static void SetStateDependentValue(DependencyObject obj, object value) =>
        obj.SetValue(StateDependentValueProperty, value);

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

    private static void OnCurrentStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (GetEnable(element))
        {
            UpdateUIBasedOnCurrentState(element);
            UpdateDependentValue(element, GetStateInfos(element), GetCurrentState(element));
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

        ButtonStateCollection stateInfos = GetStateInfos(element);
        byte currentState = GetCurrentState(element);
        Color? mouseEnterColor = GetMouseEnterColor(element);
        string hoverPopupText = stateInfos[currentState].HoverPopupText;
        string defaultHoverPopupText = GetDefaultHoverPopupText(element);

        if (mouseEnterColor is not null && stateInfos[currentState].ChangeColorOnHover)
        {
            ColorAnimations.AnimateForeground(element, (Color)mouseEnterColor, .1);
        }

        UpdatePopupText(hoverPopupText, defaultHoverPopupText);

        if (!string.IsNullOrEmpty(hoverPopupText) || !string.IsNullOrEmpty(defaultHoverPopupText))
        {
            HoverPopupHelper.DisplayPopupText(element, PlacementMode.Top);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ButtonStateCollection stateInfos = GetStateInfos(element);
        byte currentState = GetCurrentState(element);
        Color? mouseLeaveColor = GetMouseLeaveColor(element);

        if (mouseLeaveColor is not null && stateInfos[currentState].ChangeColorOnHover)
        {
            ColorAnimations.AnimateForeground(element, (Color)mouseLeaveColor, .1);
        }

        HoverPopupHelper.HidePopup();
    }

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        ButtonStateCollection stateInfos = GetStateInfos(element);
        byte currentState = GetCurrentState(element);
        byte stateQuantity = GetStateQuantity(element);

        if (currentState == stateQuantity - 1)
        {
            currentState = 0;
            SetCurrentState(element, currentState);
        }
        else
        {
            SetCurrentState(element, ++currentState);
        }

        UpdatePopupText(stateInfos[currentState].HoverPopupText, GetDefaultHoverPopupText(element));
    }

    private static void OnLoad(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        UpdateUIBasedOnCurrentState(element);
        UpdateDependentValue(element, GetStateInfos(element), GetCurrentState(element));
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

    private static void UpdateUIBasedOnCurrentState(Button element)
    {
        ButtonStateCollection stateInfos = GetStateInfos(element);
        if (stateInfos is null)
        {
            return;
        }

        byte currentState = GetCurrentState(element);
        Color? stateColor = stateInfos[currentState].Color;
        Color? mouseEnterColor = GetMouseEnterColor(element);
        FrameworkElement dependentElement = GetStateDependentElement(element);

        if (element.IsMouseOver && stateInfos[currentState].ChangeColorOnHover && mouseEnterColor is not null)
        {
            ColorAnimations.AnimateForeground(element, (Color)mouseEnterColor, .1);
        }
        else if (stateColor is not null)
        {
            ColorAnimations.AnimateForeground(element, (Color)stateColor, .1);
        }

        UpdateDependentElement(stateInfos, currentState, dependentElement);
    }

    private static void UpdateDependentElement(ButtonStateCollection stateInfos, byte currentState, FrameworkElement dependentElement)
    {
        if (dependentElement is not null)
        {
            Visibility? stateVisibility = stateInfos[currentState].DependentElementVisibility;

            if (stateVisibility is not null)
            {
                dependentElement.Visibility = (Visibility)stateVisibility!;
            }

            // Other properties can be set here as needed
        }
    }

    private static void UpdateDependentValue(Button element, ButtonStateCollection stateInfos, byte currentState)
    {
        if (stateInfos is null)
        {
            return;
        }

        object dependentValue = GetStateDependentValue(element);
        bool? stateBoolean = stateInfos[currentState].DependentValueBoolean;
        string? stateString = stateInfos[currentState].DependentValueString;

        if (dependentValue is bool v
            && stateBoolean is not null
            && v == stateBoolean)
        {
            SetStateDependentValue(element, stateBoolean!);
        }
        else if (stateString is not null)
        {
            SetStateDependentValue(element, stateString!);
        }

        // Other properties can be set here as needed
    }

    private static void UpdatePopupText(string hoverPopupText, string defaultHoverPopupText)
    {
        if (!string.IsNullOrEmpty(hoverPopupText))
        {
            HoverPopupHelper.PopupText = hoverPopupText;
        }
        else if (!string.IsNullOrEmpty(defaultHoverPopupText))
        {
            HoverPopupHelper.PopupText = defaultHoverPopupText;
        }
    }

    #endregion
}
