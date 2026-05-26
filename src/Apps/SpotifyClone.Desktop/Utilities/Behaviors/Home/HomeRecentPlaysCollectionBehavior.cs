using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Home;

public static class HomeRecentPlaysCollectionBehavior
{
    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(HomeRecentPlaysCollectionBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty ColumnCountProperty =
        DependencyProperty.RegisterAttached(
        "ColumnCount",
        typeof(int),
        typeof(HomeRecentPlaysCollectionBehavior),
        new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty StickyHeaderBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
        "StickyHeaderBackgroundColor",
        typeof(System.Windows.Media.Color),
        typeof(HomeRecentPlaysCollectionBehavior),
        new PropertyMetadata(Colors.Transparent));

    public static readonly DependencyProperty ScrollerBackgroundColorProperty =
        DependencyProperty.RegisterAttached(
        "ScrollerBackgroundColor",
        typeof(System.Windows.Media.Color),
        typeof(HomeRecentPlaysCollectionBehavior),
        new PropertyMetadata(Colors.Transparent));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static int GetColumnCount(DependencyObject obj) =>
        (int)obj.GetValue(ColumnCountProperty);
    public static void SetColumnCount(DependencyObject obj, int value) =>
        obj.SetValue(ColumnCountProperty, value);

    public static System.Windows.Media.Color GetStickyHeaderBackgroundColor(DependencyObject obj) =>
        (System.Windows.Media.Color)obj.GetValue(StickyHeaderBackgroundColorProperty);
    public static void SetStickyHeaderBackgroundColor(DependencyObject obj, System.Windows.Media.Color value) =>
        obj.SetValue(StickyHeaderBackgroundColorProperty, value);

    public static System.Windows.Media.Color GetScrollerBackgroundColor(DependencyObject obj) =>
        (System.Windows.Media.Color)obj.GetValue(ScrollerBackgroundColorProperty);
    public static void SetScrollerBackgroundColor(DependencyObject obj, System.Windows.Media.Color value) =>
        obj.SetValue(ScrollerBackgroundColorProperty, value);

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

        SetColumnCount(element, e.NewSize.Width >= 1000 ? 4 : 2);
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        string? projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
        if (projectDirectory == null)
        {
            return;
        }

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

        // Set the background of both Main and Sticky Headers
        SetStickyHeaderBackgroundColor(element, brush.Color);
        SetScrollerBackgroundColor(element, brush.Color);
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
