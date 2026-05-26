using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Playlist;

public static class PlaylistSizeChangeBehavior
{
    #region Fields

    private const double PLAYLIST_TITLE_MAX_FONT_SIZE = 50;
    private const double PLAYLIST_TITLE_MIN_FONT_SIZE = 25;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(PlaylistSizeChangeBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PlaylistTitleFontSizeProperty =
        DependencyProperty.RegisterAttached(
        "PlaylistTitleFontSize",
        typeof(double),
        typeof(PlaylistSizeChangeBehavior),
        new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty StickyHeaderBgBackgroundProperty =
        DependencyProperty.RegisterAttached(
        "StickyHeaderBgBackground",
        typeof(System.Windows.Media.Brush),
        typeof(PlaylistSizeChangeBehavior),
        new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ScrollerBgColorProperty =
        DependencyProperty.RegisterAttached(
        "ScrollerBgColor",
        typeof(System.Windows.Media.Color),
        typeof(PlaylistSizeChangeBehavior),
        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static double GetPlaylistTitleFontSize(DependencyObject obj) =>
        (double)obj.GetValue(PlaylistTitleFontSizeProperty);
    public static void SetPlaylistTitleFontSize(DependencyObject obj, double value) =>
        obj.SetValue(PlaylistTitleFontSizeProperty, value);

    public static System.Windows.Media.Brush GetStickyHeaderBgBackground(DependencyObject obj) =>
        (System.Windows.Media.Brush)obj.GetValue(StickyHeaderBgBackgroundProperty);
    public static void SetStickyHeaderBgBackground(DependencyObject obj, System.Windows.Media.Brush value) =>
        obj.SetValue(StickyHeaderBgBackgroundProperty, value);

    public static System.Windows.Media.Color GetScrollerBgColor(DependencyObject obj) =>
        (System.Windows.Media.Color)obj.GetValue(ScrollerBgColorProperty);
    public static void SetScrollerBgColor(DependencyObject obj, System.Windows.Media.Color value) =>
        obj.SetValue(ScrollerBgColorProperty, value);

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

        double newFontSize = e.NewSize.Width / 22;

        if (newFontSize <= PLAYLIST_TITLE_MAX_FONT_SIZE && newFontSize >= PLAYLIST_TITLE_MIN_FONT_SIZE)
        {
            SetPlaylistTitleFontSize(element, newFontSize);
        }
        else if (newFontSize >= PLAYLIST_TITLE_MAX_FONT_SIZE)
        {
            SetPlaylistTitleFontSize(element, PLAYLIST_TITLE_MAX_FONT_SIZE);
        }
        else if (newFontSize <= PLAYLIST_TITLE_MIN_FONT_SIZE)
        {
            SetPlaylistTitleFontSize(element, PLAYLIST_TITLE_MIN_FONT_SIZE);
        }
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not UserControl element)
        {
            return;
        }

        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
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

        // Set the background
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
