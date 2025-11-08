function setTrackContainerHeights() {
  document.querySelectorAll('.track-items').forEach(container => {
    const firstItem = container.querySelector('.track-item');
    if (firstItem) {
      container.style.height = firstItem.offsetHeight + 'px';
    }
  });
}

window.addEventListener('load', setTrackContainerHeights);
window.addEventListener('resize', setTrackContainerHeights);
