using SpotifyClone.Desktop.Utilities.Enums.Playlist;
using System.Windows;
using System.Windows.Controls;

namespace SpotifyClone.Desktop.Utilities.Behaviors.Playlist.PlaylistHeader;

public static class PlaylistHeaderItemBehavior
{
    #region Fields

    // donno if these are necessary, but they are here just in case
    //private static string _titleText = string.Empty;
    //private static string _titleSortIcon = string.Empty;
    //private static string _albumSortIcon = string.Empty;
    //private static string _dateSortIcon = string.Empty;
    //private static string _durationSortIcon = string.Empty;

    private static PlaylistSortType _sortType = PlaylistSortType.Off;
    private static Button _titleButton = null!;
    private static Button _albumButton = null!;
    private static Button _dateButton = null!;
    private static Button _durationButton = null!;

    #endregion

    #region Properties

    private static Visibility TitleSortVisibility
    {
        get;
        set
        {
            field = value;
            SetTitleSortVisibility(_titleButton, value);
        }
    } = Visibility.Visible;

    private static Visibility AlbumSortVisibility
    {
        get;
        set
        {
            field = value;
            SetAlbumSortVisibility(_albumButton, value);
        }
    } = Visibility.Visible;

    private static Visibility DateSortVisibility
    {
        get;
        set
        {
            field = value;
            SetDateSortVisibility(_dateButton, value);
        }
    } = Visibility.Visible;

    private static Visibility DurationSortVisibility
    {
        get;
        set
        {
            field = value;
            SetDurationSortVisibility(_durationButton, value);
        }
    } = Visibility.Visible;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty EnableProperty =
        DependencyProperty.RegisterAttached(
        "Enable",
        typeof(bool),
        typeof(PlaylistHeaderItemBehavior),
        new PropertyMetadata(false, OnEnableChanged));

    public static readonly DependencyProperty SortTypeProperty =
        DependencyProperty.RegisterAttached(
        "SortType",
        typeof(PlaylistSortType),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(PlaylistSortType.Off, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSortValueChanged));

    public static readonly DependencyProperty TitleSortVisibilityProperty =
        DependencyProperty.RegisterAttached(
        "TitleSortVisibility",
        typeof(Visibility),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTitleSortVisibilityChanged));

    public static readonly DependencyProperty AlbumSortVisibilityProperty =
        DependencyProperty.RegisterAttached(
        "AlbumSortVisibility",
        typeof(Visibility),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAlbumSortVisibilityChanged));

    public static readonly DependencyProperty DateSortVisibilityProperty =
        DependencyProperty.RegisterAttached(
        "DateSortVisibility",
        typeof(Visibility),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDateSortVisibilityChanged));

    public static readonly DependencyProperty DurationSortVisibilityProperty =
        DependencyProperty.RegisterAttached(
        "DurationSortVisibility",
        typeof(Visibility),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDurationSortVisibilityChanged));

    public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.RegisterAttached(
        "TitleText",
        typeof(string),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTitleTextChanged));

    public static readonly DependencyProperty TitleSortIconProperty =
        DependencyProperty.RegisterAttached(
        "TitleSortIcon",
        typeof(string),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTitleSortIconChanged));

    public static readonly DependencyProperty AlbumSortIconProperty =
        DependencyProperty.RegisterAttached(
        "AlbumSortIcon",
        typeof(string),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAlbumSortIconChanged));

    public static readonly DependencyProperty DateSortIconProperty =
        DependencyProperty.RegisterAttached(
        "DateSortIcon",
        typeof(string),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDateSortIconChanged));

    public static readonly DependencyProperty DurationSortIconProperty =
        DependencyProperty.RegisterAttached(
        "DurationSortIcon",
        typeof(string),
        typeof(PlaylistHeaderItemBehavior),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDurationSortIconChanged));

    public static readonly DependencyProperty TitleButtonProperty =
        DependencyProperty.RegisterAttached(
        "TitleButton",
        typeof(Button),
        typeof(PlaylistHeaderItemBehavior),
        new PropertyMetadata(null, OnTitleButtonChanged));

    public static readonly DependencyProperty AlbumButtonProperty =
        DependencyProperty.RegisterAttached(
        "AlbumButton",
        typeof(Button),
        typeof(PlaylistHeaderItemBehavior),
        new PropertyMetadata(null, OnAlbumButtonChanged));

    public static readonly DependencyProperty DateButtonProperty =
        DependencyProperty.RegisterAttached(
        "DateButton",
        typeof(Button),
        typeof(PlaylistHeaderItemBehavior),
        new PropertyMetadata(null, OnDateButtonChanged));

    public static readonly DependencyProperty DurationButtonProperty =
        DependencyProperty.RegisterAttached(
        "DurationButton",
        typeof(Button),
        typeof(PlaylistHeaderItemBehavior),
        new PropertyMetadata(null, OnDurationButtonChanged));

    #endregion

    #region Getters/Setters

    public static bool GetEnable(DependencyObject obj) =>
        (bool)obj.GetValue(EnableProperty);
    public static void SetEnable(DependencyObject obj, bool value) =>
        obj.SetValue(EnableProperty, value);

    public static PlaylistSortType GetSortType(DependencyObject obj) =>
        (PlaylistSortType)obj.GetValue(SortTypeProperty);
    public static void SetSortType(DependencyObject obj, PlaylistSortType value) =>
        obj.SetValue(SortTypeProperty, value);

    public static Visibility GetTitleSortVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(TitleSortVisibilityProperty);
    public static void SetTitleSortVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(TitleSortVisibilityProperty, value);

    public static Visibility GetAlbumSortVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(AlbumSortVisibilityProperty);
    public static void SetAlbumSortVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(AlbumSortVisibilityProperty, value);

    public static Visibility GetDateSortVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(DateSortVisibilityProperty);
    public static void SetDateSortVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(DateSortVisibilityProperty, value);

    public static Visibility GetDurationSortVisibility(DependencyObject obj) =>
        (Visibility)obj.GetValue(DurationSortVisibilityProperty);
    public static void SetDurationSortVisibility(DependencyObject obj, Visibility value) =>
        obj.SetValue(DurationSortVisibilityProperty, value);

    public static string GetTitleText(DependencyObject obj) =>
        (string)obj.GetValue(TitleTextProperty);
    public static void SetTitleText(DependencyObject obj, string value) =>
        obj.SetValue(TitleTextProperty, value);

    public static string GetTitleSortIcon(DependencyObject obj) =>
        (string)obj.GetValue(TitleSortIconProperty);
    public static void SetTitleSortIcon(DependencyObject obj, string value) =>
        obj.SetValue(TitleSortIconProperty, value);

    public static string GetAlbumSortIcon(DependencyObject obj) =>
        (string)obj.GetValue(AlbumSortIconProperty);
    public static void SetAlbumSortIcon(DependencyObject obj, string value) =>
        obj.SetValue(AlbumSortIconProperty, value);

    public static string GetDateSortIcon(DependencyObject obj) =>
        (string)obj.GetValue(DateSortIconProperty);
    public static void SetDateSortIcon(DependencyObject obj, string value) =>
        obj.SetValue(DateSortIconProperty, value);

    public static string GetDurationSortIcon(DependencyObject obj) =>
        (string)obj.GetValue(DurationSortIconProperty);
    public static void SetDurationSortIcon(DependencyObject obj, string value) =>
        obj.SetValue(DurationSortIconProperty, value);

    public static Button GetTitleButton(DependencyObject obj) =>
        (Button)obj.GetValue(TitleButtonProperty);
    public static void SetTitleButton(DependencyObject obj, Button value) =>
        obj.SetValue(TitleButtonProperty, value);

    public static Button GetAlbumButton(DependencyObject obj) =>
        (Button)obj.GetValue(AlbumButtonProperty);
    public static void SetAlbumButton(DependencyObject obj, Button value) =>
        obj.SetValue(AlbumButtonProperty, value);

    public static Button GetDateButton(DependencyObject obj) =>
        (Button)obj.GetValue(DateButtonProperty);
    public static void SetDateButton(DependencyObject obj, Button value) =>
        obj.SetValue(DateButtonProperty, value);

    public static Button GetDurationButton(DependencyObject obj) =>
        (Button)obj.GetValue(DurationButtonProperty);
    public static void SetDurationButton(DependencyObject obj, Button value) =>
        obj.SetValue(DurationButtonProperty, value);

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
            element.Click += OnClick;
            element.Unloaded += DetachEvents;
        }
        else
        {
            element.Click -= OnClick;
            element.Unloaded -= DetachEvents;
        }
    }

    private static void OnSortValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element))
        {
            return;
        }

        _sortType = (PlaylistSortType)e.NewValue;

        switch (_sortType)
        {
            case PlaylistSortType.Off:
                SortOff(element);
                break;
            case PlaylistSortType.TitleAsc:
                SortTitleByAscending(element);
                break;
            case PlaylistSortType.TitleDesc:
                SortTitleByDescending(element);
                break;
            case PlaylistSortType.ArtistAsc:
                SortArtistByAscending(element);
                break;
            case PlaylistSortType.ArtistDesc:
                SortArtistByDescending(element);
                break;
            case PlaylistSortType.AlbumAsc:
                SortAlbumByAscending(element);
                break;
            case PlaylistSortType.AlbumDesc:
                SortAlbumByDescending(element);
                break;
            case PlaylistSortType.DateAsc:
                SortDateByAscending(element);
                break;
            case PlaylistSortType.DateDesc:
                SortDateByDescending(element);
                break;
            case PlaylistSortType.DurationAsc:
                SortDurationByAscending(element);
                break;
            case PlaylistSortType.DurationDesc:
                SortDurationByDescending(element);
                break;
        }
    }

    private static void OnTitleSortVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element))
        {
            return;
        }

        TitleSortVisibility = (Visibility)e.NewValue;
    }

    private static void OnAlbumSortVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element))
        {
            return;
        }

        AlbumSortVisibility = (Visibility)e.NewValue;
    }

    private static void OnDateSortVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element))
        {
            return;
        }

        DateSortVisibility = (Visibility)e.NewValue;
    }

    private static void OnDurationSortVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element))
        {
            return;
        }

        DurationSortVisibility = (Visibility)e.NewValue;
    }

    private static void OnTitleTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //if (d is not Button element)
        //{
        //    return;
        //}

        //if (!GetEnable(element))
        //{
        //    return;
        //}

        //_titleText = (string)e.NewValue;
    }

    private static void OnTitleSortIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //if (d is not Button element)
        //{
        //    return;
        //}

        //if (!GetEnable(element))
        //{
        //    return;
        //}

        //_titleSortIcon = (string)e.NewValue;
    }

    private static void OnAlbumSortIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //if (d is not Button element)
        //{
        //    return;
        //}

        //if (!GetEnable(element))
        //{
        //    return;
        //}

        //_albumSortIcon = (string)e.NewValue;
    }

    private static void OnDateSortIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //if (d is not Button element)
        //{
        //    return;
        //}

        //if (!GetEnable(element))
        //{
        //    return;
        //}

        //_dateSortIcon = (string)e.NewValue;
    }

    private static void OnDurationSortIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //if (d is not Button element)
        //{
        //    return;
        //}

        //if (!GetEnable(element))
        //{
        //    return;
        //}

        //_durationSortIcon = (string)e.NewValue;
    }

    private static void OnTitleButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element) || e.NewValue is not Button button)
        {
            return;
        }

        _titleButton = button;
    }

    private static void OnAlbumButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element) || e.NewValue is not Button button)
        {
            return;
        }

        _albumButton = button;
    }

    private static void OnDateButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element) || e.NewValue is not Button button)
        {
            return;
        }

        _dateButton = button;
    }

    private static void OnDurationButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Button element)
        {
            return;
        }

        if (!GetEnable(element) || e.NewValue is not Button button)
        {
            return;
        }

        _durationButton = button;
    }

    #endregion

    #region Event Handlers

    private static void OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        if (element.Name.Equals(_titleButton.Name, StringComparison.Ordinal))
        {
            switch (_sortType)
            {
                case PlaylistSortType.TitleAsc:
                    SetSortType(element, PlaylistSortType.TitleDesc);
                    break;
                case PlaylistSortType.TitleDesc:
                    SetSortType(element, PlaylistSortType.ArtistAsc);
                    break;
                case PlaylistSortType.ArtistAsc:
                    SetSortType(element, PlaylistSortType.ArtistDesc);
                    break;
                case PlaylistSortType.ArtistDesc:
                    SetSortType(element, PlaylistSortType.Off);
                    break;
                default:
                    SetSortType(element, PlaylistSortType.TitleAsc);
                    break;
            }
        }
        else if (element.Name.Equals(_albumButton.Name, StringComparison.Ordinal))
        {
            switch (_sortType)
            {
                case PlaylistSortType.AlbumAsc:
                    SetSortType(element, PlaylistSortType.AlbumDesc);
                    break;
                case PlaylistSortType.AlbumDesc:
                    SetSortType(element, PlaylistSortType.Off);
                    break;
                default:
                    SetSortType(element, PlaylistSortType.AlbumAsc);
                    break;
            }
        }
        else if (element.Name.Equals(_dateButton.Name, StringComparison.Ordinal))
        {
            switch (_sortType)
            {
                case PlaylistSortType.DateAsc:
                    SetSortType(element, PlaylistSortType.DateDesc);
                    break;
                case PlaylistSortType.DateDesc:
                    SetSortType(element, PlaylistSortType.Off);
                    break;
                default:
                    SetSortType(element, PlaylistSortType.DateAsc);
                    break;
            }
        }
        else if (element.Name.Equals(_durationButton.Name, StringComparison.Ordinal))
        {
            switch (_sortType)
            {
                case PlaylistSortType.DurationAsc:
                    SetSortType(element, PlaylistSortType.DurationDesc);
                    break;
                case PlaylistSortType.DurationDesc:
                    SetSortType(element, PlaylistSortType.Off);
                    break;
                default:
                    SetSortType(element, PlaylistSortType.DurationAsc);
                    break;
            }
        }
        else
        {
            MessageBox.Show("Unknown button clicked.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void DetachEvents(object sender, RoutedEventArgs e)
    {
        if (sender is not Button element)
        {
            return;
        }

        element.Click -= OnClick;
        element.Unloaded -= DetachEvents;

        SetEnable(element, false);
    }

    #endregion

    #region Methods

    private static void SortOff(Button element)
    {
        SetTitleText(element, "Title");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortTitleByAscending(Button element)
    {
        SetTitleText(element, "Title");
        SetTitleSortIcon(element, "\uf0d8");

        SetTitleSortVisibility(element, Visibility.Visible);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortTitleByDescending(Button element)
    {
        SetTitleText(element, "Title");
        SetTitleSortIcon(element, "\uf0d7");

        SetTitleSortVisibility(element, Visibility.Visible);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortArtistByAscending(Button element)
    {
        SetTitleText(element, "Artist");
        SetTitleSortIcon(element, "\uf0d8");

        SetTitleSortVisibility(element, Visibility.Visible);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortArtistByDescending(Button element)
    {
        SetTitleText(element, "Artist");
        SetTitleSortIcon(element, "\uf0d7");

        SetTitleSortVisibility(element, Visibility.Visible);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortAlbumByAscending(Button element)
    {
        SetAlbumSortIcon(element, "\uf0d8");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Visible);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortAlbumByDescending(Button element)
    {
        SetAlbumSortIcon(element, "\uf0d7");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Visible);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortDateByAscending(Button element)
    {
        SetDateSortIcon(element, "\uf0d8");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Visible);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortDateByDescending(Button element)
    {
        SetDateSortIcon(element, "\uf0d7");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Visible);
        SetDurationSortVisibility(element, Visibility.Hidden);
    }

    private static void SortDurationByAscending(Button element)
    {
        SetDurationSortIcon(element, "\uf0d8");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Visible);
    }

    private static void SortDurationByDescending(Button element)
    {
        SetDurationSortIcon(element, "\uf0d7");

        SetTitleSortVisibility(element, Visibility.Hidden);
        SetAlbumSortVisibility(element, Visibility.Hidden);
        SetDateSortVisibility(element, Visibility.Hidden);
        SetDurationSortVisibility(element, Visibility.Visible);
    }

    #endregion
}
