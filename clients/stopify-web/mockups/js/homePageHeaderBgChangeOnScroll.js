const mainView = document.getElementById("main-view");
const header = document.getElementById("home-page__header");

mainView.addEventListener("scroll", () => {
  const scrollY = mainView.scrollTop;
  const fadeDistance = 100;
  const opacity = Math.min(scrollY / fadeDistance, 1);

  header.style.backgroundColor = `rgba(0, 0, 0, ${opacity})`;
});
