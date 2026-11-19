using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Artist;

public static class ArtistSizeChangeBehavior
{
    #region Fields

    private const double ArtistNameMaxFontSize = 95;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(ArtistSizeChangeBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty StickyHeaderBgBackgroundProperty =
        DependencyProperty.RegisterAttached(
        "StickyHeaderBgBackground",
        typeof(System.Windows.Media.Brush),
        typeof(ArtistSizeChangeBehavior),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ScrollerBgColorProperty =
        DependencyProperty.RegisterAttached(
        "ScrollerBgColor",
        typeof(System.Windows.Media.Color),
        typeof(ArtistSizeChangeBehavior),
        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ArtistNameFontSizeProperty =
        DependencyProperty.RegisterAttached(
        "ArtistNameFontSize",
        typeof(double),
        typeof(ArtistSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ArtistNameMaxHeightProperty =
        DependencyProperty.RegisterAttached(
        "ArtistNameMaxHeight",
        typeof(double),
        typeof(ArtistSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static System.Windows.Media.Brush GetStickyHeaderBgBackground(DependencyObject obj) =>
        (System.Windows.Media.Brush)obj.GetValue(StickyHeaderBgBackgroundProperty);
    public static void SetStickyHeaderBgBackground(DependencyObject obj, System.Windows.Media.Brush value) =>
        obj.SetValue(StickyHeaderBgBackgroundProperty, value);

    public static System.Windows.Media.Color GetScrollerBgColor(DependencyObject obj) =>
        (System.Windows.Media.Color)obj.GetValue(ScrollerBgColorProperty);
    public static void SetScrollerBgColor(DependencyObject obj, System.Windows.Media.Color value) =>
        obj.SetValue(ScrollerBgColorProperty, value);

    public static double GetArtistNameFontSize(DependencyObject obj) =>
        (double)obj.GetValue(ArtistNameFontSizeProperty);
    public static void SetArtistNameFontSize(DependencyObject obj, double value) =>
        obj.SetValue(ArtistNameFontSizeProperty, value);

    public static double GetArtistNameMaxHeight(DependencyObject obj) =>
        (double)obj.GetValue(ArtistNameMaxHeightProperty);
    public static void SetArtistNameMaxHeight(DependencyObject obj, double value) =>
        obj.SetValue(ArtistNameMaxHeightProperty, value);

    #endregion

    #region Property Callbacks

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UserControl element)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            element.SizeChanged += OnSizeChanged;
            element.Loaded += OnLoaded;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.SizeChanged -= OnSizeChanged;
            element.Loaded -= OnLoaded;
            element.Unloaded -= DetachEvents;
        }
    }

    #endregion

    #region Event Handlers

    private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        double newFontSize = e.NewSize.Width / 11;
        SetArtistNameFontSize(element, newFontSize >= ArtistNameMaxFontSize ? ArtistNameMaxFontSize : newFontSize);
        SetArtistNameMaxHeight(element, newFontSize * 1.2);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        string imagePath = Path.Combine(projectDirectory, "Assets", "Images", "song.jpg");

        using var bitmap = new Bitmap(imagePath);

        // Variables to store sum of RGB components
        long rSum = 0;
        long gSum = 0;
        long bSum = 0;
        int totalPixels = 0;

        // Loop through each pixel in the image
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                // Get pixel color
                System.Drawing.Color pixelColor = bitmap.GetPixel(x, y);

                // Sum up RGB components
                rSum += pixelColor.R;
                gSum += pixelColor.G;
                bSum += pixelColor.B;
                totalPixels++;
            }
        }

        // Calculate average RGB values
        byte avgR = (byte)(rSum / totalPixels);
        byte avgG = (byte)(gSum / totalPixels);
        byte avgB = (byte)(bSum / totalPixels);

        // Convert the average color to WPF Color and Brush
        var averageColor = System.Windows.Media.Color.FromRgb(avgR, avgG, avgB);
        var brush = new SolidColorBrush(averageColor);

        // Set the background colors
        SetStickyHeaderBgBackground(element, brush);
        SetScrollerBgColor(element, brush.Color);
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        element.SizeChanged -= OnSizeChanged;
        element.Loaded -= OnLoaded;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
