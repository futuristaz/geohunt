let remainingSeconds = 120; 

function updateClock() {
  let minutes = Math.floor(remainingSeconds / 60);
  let seconds = remainingSeconds % 60;
  document.getElementById("clock").textContent =
    String(minutes).padStart(2, "0") + ":" + String(seconds).padStart(2, "0");

  if (remainingSeconds > 0) {
    remainingSeconds--;
  }
}

updateClock();
setInterval(updateClock, 1000);
