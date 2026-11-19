const playlistScrollView = document.getElementById("main-view");
const playlistHeader = document.getElementById("playlist-page__header");

playlistScrollView.addEventListener("scroll", () => {
  const scrollY = playlistScrollView.scrollTop;
  const fadeDistance = 300;
  const opacity = Math.min(scrollY / fadeDistance, 1);

  playlistHeader.style.opacity = opacity;
});
