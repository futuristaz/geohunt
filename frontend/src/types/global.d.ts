export {};

declare global {
  interface Window {
    google?: typeof google;

    __googleMapsReady?: Promise<void>;
    __resolveGoogleMapsReady?: () => void;
    __rejectGoogleMapsReady?: (reason?: unknown) => void;
    __onGoogleMapsLoaded?: () => void;
  }
}

