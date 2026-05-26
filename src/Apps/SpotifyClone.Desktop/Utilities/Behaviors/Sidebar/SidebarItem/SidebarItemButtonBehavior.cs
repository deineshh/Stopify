using SpotifyClone.Desktop.Utilities.Helpers;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Sidebar.SidebarItem;

public static class SidebarItemButtonBehavior
{
    #region Fields

    private static readonly TextBlock _playlistTitle = new()
    {
        Foreground = Brushes.White,
        FontWeight = FontWeights.SemiBold,
        FontSize = 15
    };

    private static readonly TextBlock _playlistInfo = new()
    {
        Foreground = Brushes.DarkGray,
        FontWeight = FontWeights.SemiBold,
        FontSize = 13
    };

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty PlaylistTitleProperty =
        DependencyProperty.RegisterAttached(
            "PlaylistTitle",
            typeof(string),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(string.Empty, OnPlaylistTitleChanged));

    public static readonly DependencyProperty PlaylistTypeProperty =
        DependencyProperty.RegisterAttached(
            "PlaylistType",
            typeof(string),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(string.Empty, OnPlaylistTypeChanged));

    public static readonly DependencyProperty PlaylistAuthorProperty =
        DependencyProperty.RegisterAttached(
            "PlaylistAuthor",
            typeof(string),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(string.Empty, OnPlaylistAuthorChanged));

    public static readonly DependencyProperty PlaylistSongQuantityProperty =
        DependencyProperty.RegisterAttached(
            "PlaylistSongQuantity",
            typeof(int),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(0));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.RegisterAttached(
            "Description",
            typeof(string),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlayBtnWidthProperty =
        DependencyProperty.RegisterAttached(
            "PlayBtnWidth",
            typeof(double),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlayBtnContentProperty =
        DependencyProperty.RegisterAttached(
            "PlayBtnContent",
            typeof(string),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlayBtnFontSizeProperty =
        DependencyProperty.RegisterAttached(
            "PlayBtnFontSize",
            typeof(double),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ItemImgBtnOpacityProperty =
        DependencyProperty.RegisterAttached(
            "ItemImgBtnOpacity",
            typeof(double),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SidebarPlayingIconWidthProperty =
        DependencyProperty.RegisterAttached(
            "SidebarPlayingIconWidth",
            typeof(double),
            typeof(SidebarItemButtonBehavior),
            new FrameworkPropertyMetadata((double)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsPlaying =
        DependencyProperty.RegisterAttached(
            "IsPlaying",
            typeof(bool),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(false, OnIsPlayingChanged));

    public static readonly DependencyProperty SidebarCollapseStateProperty =
        DependencyProperty.RegisterAttached(
            "SidebarCollapseState",
            typeof(bool),
            typeof(SidebarItemButtonBehavior),
            new PropertyMetadata(false));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static string GetPlaylistTitle(DependencyObject obj) =>
        (string)obj.GetValue(PlaylistTitleProperty);
    public static void SetPlaylistTitle(DependencyObject obj, string value) =>
        obj.SetValue(PlaylistTitleProperty, value);

    public static string GetPlaylistType(DependencyObject obj) =>
        (string)obj.GetValue(PlaylistTypeProperty);
    public static void SetPlaylistType(DependencyObject obj, string value) =>
        obj.SetValue(PlaylistTypeProperty, value);

    public static string GetPlaylistAuthor(DependencyObject obj) =>
        (string)obj.GetValue(PlaylistAuthorProperty);
    public static void SetPlaylistAuthor(DependencyObject obj, string value) =>
        obj.SetValue(PlaylistAuthorProperty, value);

    public static int GetPlaylistSongQuantity(DependencyObject obj) =>
        (int)obj.GetValue(PlaylistSongQuantityProperty);
    public static void SetPlaylistSongQuantity(DependencyObject obj, int value) =>
        obj.SetValue(PlaylistSongQuantityProperty, value);

    public static string GetDescription(DependencyObject obj) =>
        (string)obj.GetValue(DescriptionProperty);
    public static void SetDescription(DependencyObject obj, string value) =>
        obj.SetValue(DescriptionProperty, value);

    public static double GetPlayBtnWidth(DependencyObject obj) =>
        (double)obj.GetValue(PlayBtnWidthProperty);
    public static void SetPlayBtnWidth(DependencyObject obj, double value) =>
        obj.SetValue(PlayBtnWidthProperty, value);

    public static string GetPlayBtnContent(DependencyObject obj) =>
        (string)obj.GetValue(PlayBtnContentProperty);
    public static void SetPlayBtnContent(DependencyObject obj, string value) =>
        obj.SetValue(PlayBtnContentProperty, value);

    public static double GetPlayBtnFontSize(DependencyObject obj) =>
        (double)obj.GetValue(PlayBtnFontSizeProperty);
    public static void SetPlayBtnFontSize(DependencyObject obj, double value) =>
        obj.SetValue(PlayBtnFontSizeProperty, value);

    public static double GetItemImgBtnOpacity(DependencyObject obj) =>
        (double)obj.GetValue(ItemImgBtnOpacityProperty);
    public static void SetItemImgBtnOpacity(DependencyObject obj, double value) =>
        obj.SetValue(ItemImgBtnOpacityProperty, value);

    public static double GetSidebarPlayingIconWidth(DependencyObject obj) =>
        (double)obj.GetValue(SidebarPlayingIconWidthProperty);
    public static void SetSidebarPlayingIconWidth(DependencyObject obj, double value) =>
        obj.SetValue(SidebarPlayingIconWidthProperty, value);

    public static bool GetIsPlaying(DependencyObject obj) =>
        (bool)obj.GetValue(IsPlaying);
    public static void SetIsPlaying(DependencyObject obj, bool value) =>
        obj.SetValue(IsPlaying, value);

    public static bool GetSidebarCollapseState(DependencyObject obj) =>
        (bool)obj.GetValue(SidebarCollapseStateProperty);
    public static void SetSidebarCollapseState(DependencyObject obj, bool value) =>
        obj.SetValue(SidebarCollapseStateProperty, value);

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
            element.Loaded += OnLoad;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
            element.Loaded -= OnLoad;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnPlaylistTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        string playlistTitle = (string)e.NewValue;
        string playlistType = GetPlaylistType(element);
        int playlistSongQuantity = GetPlaylistSongQuantity(element);

        if (playlistTitle.Equals("Liked Songs", StringComparison.Ordinal))
        {
            string playlistInfo = $"{playlistType} • {playlistSongQuantity}";
            SetDescription(element, playlistInfo);
        }
    }

    private static void OnPlaylistTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        string playlistTitle = GetPlaylistTitle(element);
        int playlistSongQuantity = GetPlaylistSongQuantity(element);
        string playlistType = (string)e.NewValue;
        string playlistAuthor = GetPlaylistAuthor(element);

        StringBuilder playlistInfo = new(playlistType);

        if (playlistTitle.Equals("Liked Songs", StringComparison.Ordinal))
        {
            playlistInfo.Append($" • {playlistSongQuantity} songs");
        }
        else if (playlistType.Equals("Playlist", StringComparison.Ordinal)
            || playlistType.Equals("Album", StringComparison.Ordinal)
            || playlistType.Equals("Single", StringComparison.Ordinal))
        {
            playlistInfo.Append($" • {playlistAuthor}");
        }

        SetDescription(element, playlistInfo.ToString());
    }

    private static void OnPlaylistAuthorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        string playlistAuthor = (string)e.NewValue;
        string playlistTitle = GetPlaylistTitle(element);
        string playlistType = GetPlaylistType(element);

        if (!playlistTitle.Equals("Liked Songs", StringComparison.Ordinal)
            && !playlistType.Equals("Artist", StringComparison.Ordinal))
        {
            string playlistInfo = $"{playlistType} • {playlistAuthor}";
            SetDescription(element, playlistInfo);
        }
    }

    private static void OnIsPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element || !GetEnable(element))
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            SetPlayBtnContent(element, "\uf04c");
            SetPlayBtnFontSize(element, 25);
            SetSidebarPlayingIconWidth(element, 25);
        }
        else
        {
            SetPlayBtnContent(element, "\uf04b");
            SetPlayBtnFontSize(element, 22);
            SetSidebarPlayingIconWidth(element, 0);
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

        if (GetSidebarCollapseState(element))
        {
            _playlistTitle.Text = GetPlaylistTitle(element);
            _playlistInfo.Text = GetDescription(element);
            HoverPopupHelper.DisplayPopupTextBlock(element, PlacementMode.Right, _playlistTitle, _playlistInfo);
        }
        else
        {
            SetPlayBtnWidth(element, double.NaN);
            SetItemImgBtnOpacity(element, .4);
        }
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (GetSidebarCollapseState(element))
        {
            HoverPopupHelper.HidePopup();
        }
        else
        {
            SetPlayBtnWidth(element, 0);
            SetItemImgBtnOpacity(element, 1);
        }
    }

    private static void OnLoad(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (GetIsPlaying(element))
        {
            SetPlayBtnContent(element, "\uf04c");
            SetPlayBtnFontSize(element, 25);
            SetSidebarPlayingIconWidth(element, 25);
        }
        else
        {
            SetPlayBtnContent(element, "\uf04b");
            SetPlayBtnFontSize(element, 22);
            SetSidebarPlayingIconWidth(element, 0);
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
        element.Loaded -= OnLoad;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion
}
