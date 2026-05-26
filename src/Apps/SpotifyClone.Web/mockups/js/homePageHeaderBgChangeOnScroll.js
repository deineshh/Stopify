const homeScrollView = document.getElementById("main-view");
const homeHeader = document.getElementById("home-page__header");

homeScrollView.addEventListener("scroll", () => {
  const scrollY = homeScrollView.scrollTop;
  const fadeDistance = 100;
  const opacity = Math.min(scrollY / fadeDistance, 1);

  homeHeader.style.backgroundColor = `rgba(0, 0, 0, ${opacity})`;
});
