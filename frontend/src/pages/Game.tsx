declare global {
    interface Window {
        google: any;
    }
}

import { useEffect, useRef, useState } from 'react';

interface Coordinates {
  lat: number;
  lng: number;
}

interface ApiResponse {
  modifiedCoordinates: {
    lat: string | number;
    lng: string | number;
  };
}

const StreetViewApp = () => {
  const streetViewRef = useRef<HTMLDivElement>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const isInitalizedRef = useRef(false);

  useEffect(() => {
    // Prevent double initialization
    if (isInitalizedRef.current) return;
    isInitalizedRef.current = true;

    const initializeStreetView = async () => {
      try {
        // wait for Google Maps to exist
        await new Promise<void>((resolve, reject) => {
            const start = Date.now();
            const wait = () => {
                if (window.google?.maps) return resolve();
                if (Date.now() - start > 15000) return reject(new Error("Google Maps API not loaded"));
                setTimeout(wait, 100);
            };
            wait();
        });

        if (!streetViewRef.current) {
            throw new Error('Street View container not mounted');
        }

        // Check if Google Maps is available
        if (!window.google || !window.google.maps) {
          throw new Error('Google Maps API not loaded yet');
        }

        console.log('Fetching coordinates...');
        
        // Fetch coordinates from API
        const response = await fetch('/api/geocoding/valid_coords');
        
        if (!response.ok) {
          throw new Error(`API request failed: ${response.status}`);
        }

        const data: ApiResponse = await response.json();
        console.log('Received coordinates:', data);

        // Convert to numbers and validate
        const lat = parseFloat(String(data.modifiedCoordinates.lat));
        const lng = parseFloat(String(data.modifiedCoordinates.lng));

        if (isNaN(lat) || isNaN(lng)) {
          throw new Error('Invalid coordinates received from API');
        }

        const initialPosition: Coordinates = { lat, lng };
        console.log('Initializing Street View at:', initialPosition);


        new window.google.maps.StreetViewPanorama(streetViewRef.current, {
            position: { lat, lng } as Coordinates,
            pov: { heading: 0, pitch: 0},
            zoom: 1,
            addressControl: false,
            fullscreenControl: false,
            imageDateControl: false,
            showRoadLabels: false,
        });

        setLoading(false);
      } catch (err) {
        const errorMessage = err instanceof Error ? err.message : 'Failed to load Street View';
        setError(errorMessage);
        setLoading(false);
        console.error('Street View initialization error:', err);
      }
    };

    // Wait for Google Maps to be available
    const waitForGoogleMaps = () => {
      if (window.google && window.google.maps) {
        console.log('Google Maps API is ready');
        initializeStreetView();
      } else {
        console.log('Waiting for Google Maps API...');
        setTimeout(waitForGoogleMaps, 100);
      }
    };

    initializeStreetView();
  }, []);

return (
    <div className="relative h-screen w-screen">
      {/* Always render the container so the ref exists */}
      <div ref={streetViewRef} className="h-full w-full" />

      {/* Loading overlay */}
      {loading && !error && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/30 backdrop-blur-sm">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
            <p className="text-white">Loading Street View...</p>
            <p className="text-white/70 text-sm mt-2">Check console for details</p>
          </div>
        </div>
      )}

      {/* Error overlay */}
      {error && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-100">
          <div className="bg-white p-8 rounded-lg shadow-lg max-w-md">
            <h2 className="text-red-600 text-xl font-bold mb-2">Error</h2>
            <p className="text-gray-700">{error}</p>
            <button
              onClick={() => window.location.reload()}
              className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Retry
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default StreetViewApp;