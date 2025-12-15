type WaitOptions = {
  timeoutMs?: number;
};

function hasMapsConstructors(): boolean {
  return (
    typeof window.google?.maps?.Map === 'function' &&
    typeof window.google?.maps?.Marker === 'function' &&
    typeof window.google?.maps?.StreetViewPanorama === 'function'
  );
}

export async function waitForGoogleMapsApi(options: WaitOptions = {}): Promise<void> {
  const timeoutMs = options.timeoutMs ?? 15000;

  if (hasMapsConstructors()) return;

  const start = Date.now();

  // Prefer the promise created in `index.html` (callback-based).
  if (window.__googleMapsReady) {
    const remaining = Math.max(0, timeoutMs - (Date.now() - start));
    await Promise.race([
      window.__googleMapsReady,
      new Promise<void>((_, reject) =>
        setTimeout(() => reject(new Error('Google Maps API not loaded')), remaining)
      ),
    ]);

    if (hasMapsConstructors()) return;
  }

  // Fallback: wait for the DOM event (or poll if necessary).
  await new Promise<void>((resolve, reject) => {
    const onReady = () => {
      cleanup();
      resolve();
    };

    const interval = setInterval(() => {
      if (hasMapsConstructors()) {
        cleanup();
        resolve();
      } else if (Date.now() - start > timeoutMs) {
        cleanup();
        reject(new Error('Google Maps API not loaded'));
      }
    }, 100);

    const cleanup = () => {
      clearInterval(interval);
      window.removeEventListener('google-maps-ready', onReady);
    };

    window.addEventListener('google-maps-ready', onReady, { once: true });
  });
}

