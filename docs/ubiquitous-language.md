# Stopify – Ubiquitous Language

This document defines the **Ubiquitous Language** for the Stopify domain.
All team members should use these terms consistently in code, documentation, UI text, and discussions.
The purpose is to ensure shared understanding of the domain and avoid ambiguity.

---

## Core Terms

- **Album** – A collection of songs released together by one or multiple artists.
- **Artist** – A creator of songs.
- **Playlist** – A user-defined or system-generated collection of songs, stored in a specific order.
- **Song** – A single musical track with metadata such as title, duration, artist, and album.
- **User** – An individual who has registered with Stopify and can interact with the system’s features.
- **PlaybackHistory** – A record of songs the user has played, used for recommendations and analytics.

---

## Core Actions

- **Register user** – Create a new user account in the system.
- **Login user** – Authenticate a user to access their account.
- **Add song to a playlist** – Insert a specific song into an existing playlist.
- **Add song to an album** – Associate a song with an album (typically by an artist or system admin).
- **Remove song from a playlist** – Delete a specific song from a playlist.
- **Skip to next track** – Skip to the next track in the NowPlayingQueue.
- **Skip to previous track** – Return to the previous song in the NowPlayingQueue.
- **Follow an artist** – Subscribe to updates and content from a specific artist.
- **Unfollow an artist** – Stop receiving updates and content from a specific artist.

---

**Context Map**

1. **Playback Experience** (Core):
    1. **Playback Context**:
        - States: *NowPlayingQueue*;
        - Actions: PlaySong, PauseSong, NextSong, PreviousSong, ShufflePlaylist, RepeatPlaylist, RepeatOneSong;
        - Upstreams: **Catalog Context** (CS, PL), **Library Context** (CS);
2. **Personal Library** (Core):
    1. **Library Context**:
        - States: *Playlist*, *SavedAlbum*, *SavedSong* (or *LibraryEntry*);
        - Actions: AddSongToPlaylist, RemoveSongFromPlaylist, SaveSongToPlaylist, UnsaveSongFromPlaylist;
        - Depends on: **Catalog Context** (Conformist).
3. **Catalog Management** (Supporting):
    1. **Catalog Context**:
        - States: *Artist*, *Album*, *Song*, *Playlist*;
        - Actions: AddSongToAlbum, RemoveSongFromAlbum, PublishAlbum, RetireSong.
        - Depends on: ***NONE***.
4. **Listening History & Insights** (Supporting):
    1. **History Context**:
        - States: *PlaybackHistory*;
        - Actions: AddSongToHistory, RemoveSongFromHistory;
        - Depends on: **Playback Context** (ACL), **Catalog Context**.
    2. **Recents Projection Context**:
        - States: *RecentlyPlayed*;
        - Actions: GetRecentlyPlayedSongs;
        - Depends on: **History Context** (SK).
5. **Engagement** (Supporting):
    1. **Follow Context**:
        - States: *Follower*, *Follow*;
        - Actions: Follow, Unfollow;
        - Depends on: **Catalog Context** (CS).
6. **Search & Discovery** (Supporting):
    1. **Search Context**:
        - States: *SearchResult*;
        - Actions: SearchSongs, SearchArtists, SearchAlbums, SearchPlaylists;
        - Depends on: **Catalog Context** (OHS, PL), **Library Context** (CS);
7. **Identity & Access** (Generic):
    1. **Accounts Context**:
        - States: *UserAccount*;
        - Actions: Login, Register;
        - Depends on: ***NONE***.
