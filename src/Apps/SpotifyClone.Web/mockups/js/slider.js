const sliders = document.getElementsByClassName("now-playing-bar__slider--input");

function updateSliderBackground(slider, active = false) {
  const value = (slider.value / slider.max) * 100;
  const activeColor = active ? "#1db954" : "#ffffff";
  slider.style.background = `linear-gradient(to right, ${activeColor} ${value}%, #535353 ${value}%)`;
}

Array.from(sliders).forEach((slider) => {
  updateSliderBackground(slider);

  slider.addEventListener("input", (e) => {
    const active = e.target.matches(":hover");
    updateSliderBackground(e.target, active);
  });

  slider.addEventListener("mouseenter", () => updateSliderBackground(slider, true));
  slider.addEventListener("mouseleave", () => updateSliderBackground(slider, false));
});
