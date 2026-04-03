import React, { useState, useEffect } from 'react';

interface WeatherDay {
  date: string;
  tempMax: number;
  tempMin: number;
  precipitation: number;
  windSpeed: number;
  weatherCode: number;
}

interface WeatherData {
  latitude: number;
  longitude: number;
  timezone: string;
  daily: {
    time: string[];
    temperature_2m_max: number[];
    temperature_2m_min: number[];
    precipitation_sum: number[];
    windspeed_10m_max: number[];
    weather_code: number[];
  };
}

const getWeatherDescription = (code: number): string => {
  const weatherCodes: { [key: number]: string } = {
    0: 'Clear sky',
    1: 'Mainly clear',
    2: 'Partly cloudy',
    3: 'Overcast',
    45: 'Foggy',
    48: 'Depositing rime fog',
    51: 'Light drizzle',
    53: 'Moderate drizzle',
    55: 'Dense drizzle',
    61: 'Slight rain',
    63: 'Moderate rain',
    65: 'Heavy rain',
    71: 'Slight snow',
    73: 'Moderate snow',
    75: 'Heavy snow',
    77: 'Snow grains',
    80: 'Slight rain showers',
    81: 'Moderate rain showers',
    82: 'Violent rain showers',
    85: 'Slight snow showers',
    86: 'Heavy snow showers',
    95: 'Thunderstorm',
    96: 'Thunderstorm with slight hail',
    99: 'Thunderstorm with heavy hail',
  };
  return weatherCodes[code] || 'Unknown';
};

const getWeatherIcon = (code: number): string => {
  if (code === 0) return '☀️';
  if (code === 1 || code === 2) return '⛅';
  if (code === 3) return '☁️';
  if (code === 45 || code === 48) return '🌫️';
  if (code >= 51 && code <= 55) return '🌧️';
  if (code >= 61 && code <= 65) return '🌧️';
  if (code >= 71 && code <= 77) return '❄️';
  if (code >= 80 && code <= 82) return '🌧️';
  if (code >= 85 && code <= 86) return '❄️';
  if (code >= 95 && code <= 99) return '⛈️';
  return '🌤️';
};

const WeatherCard: React.FC<{ day: WeatherDay }> = ({ day }) => {
  return (
    <div className="bg-white rounded-lg shadow-md p-4 flex flex-col items-center justify-between h-full hover:shadow-lg transition-shadow">
      <div className="text-center w-full">
        <p className="text-gray-600 text-sm font-medium mb-2">
          {new Date(day.date).toLocaleDateString('en-US', {
            weekday: 'short',
            month: 'short',
            day: 'numeric',
          })}
        </p>
        <div className="text-4xl mb-3">{getWeatherIcon(day.weatherCode)}</div>
        <p className="text-xs text-gray-500 mb-3 h-8">
          {getWeatherDescription(day.weatherCode)}
        </p>
      </div>

      <div className="w-full space-y-2 text-sm">
        <div className="flex justify-between items-center">
          <span className="text-gray-600">Temperature:</span>
          <span className="font-semibold">
            {day.tempMax}°C / {day.tempMin}°C
          </span>
        </div>
        <div className="flex justify-between items-center">
          <span className="text-gray-600">Precipitation:</span>
          <span className="font-semibold">{day.precipitation.toFixed(1)} mm</span>
        </div>
        <div className="flex justify-between items-center">
          <span className="text-gray-600">Wind Speed:</span>
          <span className="font-semibold">{day.windSpeed.toFixed(1)} km/h</span>
        </div>
      </div>
    </div>
  );
};

const LoadingSpinner: React.FC = () => (
  <div className="flex justify-center items-center h-64">
    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
  </div>
);

const ErrorMessage: React.FC<{ message: string; onRetry: () => void }> = ({
  message,
  onRetry,
}) => (
  <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
    <p className="text-red-700 mb-4">{message}</p>
    <button
      onClick={onRetry}
      className="bg-red-600 hover:bg-red-700 text-white font-medium py-2 px-4 rounded transition-colors"
    >
      Try Again
    </button>
  </div>
);

export default function App() {
  const [weatherData, setWeatherData] = useState<WeatherDay[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchWeatherData = async () => {
    try {
      setLoading(true);
      setError(null);

      const apiBaseUrl =
        import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';
      const response = await fetch(`${apiBaseUrl}/api/weather/como`);

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: WeatherData = await response.json();

      const forecast: WeatherDay[] = data.daily.time.map((date, index) => ({
        date,
        tempMax: data.daily.temperature_2m_max[index],
        tempMin: data.daily.temperature_2m_min[index],
        precipitation: data.daily.precipitation_sum[index],
        windSpeed: data.daily.windspeed_10m_max[index],
        weatherCode: data.daily.weather_code[index],
      }));

      setWeatherData(forecast);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : 'Failed to fetch weather data';
      setError(errorMessage);
      console.error('Weather fetch error:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWeatherData();
  }, []);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-400 to-blue-600 p-4 md:p-8">
      <div className="max-w-7xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-bold text-white mb-2">
            Como Weather Forecast
          </h1>
          <p className="text-blue-100 text-lg">
            7-Day Weather Forecast for Como, Italy
          </p>
        </div>

        {loading && <LoadingSpinner />}

        {error && <ErrorMessage message={error} onRetry={fetchWeatherData} />}

        {!loading && !error && weatherData.length > 0 && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {weatherData.map((day) => (
              <WeatherCard key={day.date} day={day} />
            ))}
          </div>
        )}

        {!loading && !error && weatherData.length === 0 && (
          <div className="text-center text-white">
            <p className="text-lg">No weather data available</p>
          </div>
        )}
      </div>
    </div>
  );
}