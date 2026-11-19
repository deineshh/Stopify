using System.Windows;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Models;

public class ButtonStateModel : Freezable
{
    #region Dependency Properties

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register("Color", typeof(Color?), typeof(ButtonStateModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HoverPopupTextProperty =
        DependencyProperty.Register("HoverPopupText", typeof(string), typeof(ButtonStateModel),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ChangeColorOnHoverProperty =
        DependencyProperty.Register("ChangeColorOnHover", typeof(bool), typeof(ButtonStateModel),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DependentElementVisibilityProperty =
        DependencyProperty.Register("DependentElementVisibility", typeof(Visibility?), typeof(ButtonStateModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DependentValueBooleanProperty =
        DependencyProperty.Register("DependentValueBoolean", typeof(bool?), typeof(ButtonStateModel),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DependentValueStringProperty =
        DependencyProperty.Register("DependentValueString", typeof(string), typeof(ButtonStateModel),
            new PropertyMetadata(null));

    #endregion

    #region Properties

    public Color? Color
    {
        get => (Color?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string HoverPopupText
    {
        get => (string)GetValue(HoverPopupTextProperty);
        set => SetValue(HoverPopupTextProperty, value);
    }

    public bool ChangeColorOnHover
    {
        get => (bool)GetValue(ChangeColorOnHoverProperty);
        set => SetValue(ChangeColorOnHoverProperty, value);
    }

    public Visibility? DependentElementVisibility
    {
        get => (Visibility?)GetValue(DependentElementVisibilityProperty);
        set => SetValue(DependentElementVisibilityProperty, value);
    }

    public bool? DependentValueBoolean
    {
        get => (bool?)GetValue(DependentValueBooleanProperty);
        set => SetValue(DependentValueBooleanProperty, value);
    }

    public string? DependentValueString
    {
        get => (string?)GetValue(DependentValueStringProperty);
        set => SetValue(DependentValueStringProperty, value);
    }

    #endregion

    #region Methods

    protected override Freezable CreateInstanceCore()
        => new ButtonStateModel();

    #endregion
}
