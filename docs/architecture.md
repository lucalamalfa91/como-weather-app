# Como Weather Forecast Application - Architecture Document

## Overview

The Como Weather Forecast Application is a modern web application that displays a 7-day weather forecast for Como, Italy. The system consists of a React-based frontend and a .NET backend API that fetches data from the Open-Meteo weather service. The application is deployed on Microsoft Azure using Container Apps for the backend and Static Web Apps for the frontend.

The primary goal is to provide users with accurate, up-to-date weather information including temperature, precipitation, wind speed, and weather conditions in a responsive, user-friendly interface.

## Functional Requirements

### User-Facing Features

The application must display a 7-day weather forecast for Como with the following information for each day:

- Minimum and maximum temperature in Celsius
- Total precipitation amount in millimeters
- Maximum wind speed in kilometers per hour
- Weather condition represented by icons or descriptive text
- Date for each forecast day

The forecast must start from the current day and extend exactly 7 days into the future. The interface must be responsive and function properly on both mobile and desktop devices.

### Backend API Capabilities

The backend API must provide a REST endpoint that returns weather forecast data in JSON format. The endpoint must retrieve data from the Open-Meteo API and transform it into a standardized response format. The API must include a health check endpoint for monitoring purposes.

The API must handle errors gracefully and return appropriate HTTP status codes when issues occur. No authentication is required for API access.

### Data Caching

The backend must implement caching to reduce the number of requests sent to the Open-Meteo API. This improves performance and reduces external API dependencies.

## Non-Functional Requirements

### Performance Requirements

- The frontend page must load completely within 3 seconds
- The backend API must respond to requests within 2 seconds
- The health check endpoint must respond within 5 seconds

### Reliability and Availability

- The backend must maintain at least 99.5% uptime
- Auto-scaling must be configured to handle traffic spikes
- Health checks must run every 10 seconds to detect failures

### Testing and Quality

- Backend unit tests must achieve at least 80% code coverage using xUnit framework
- Frontend component tests must achieve at least 75% code coverage using Jest framework
- All tests must pass before any deployment to production

### Scalability

- The backend Container App must support auto-scaling from 1 to 3 replicas based on demand
- The frontend must be served through a CDN with caching enabled
- API responses must be cached with a default TTL of 3600 seconds

### Security

- CORS must be properly configured to allow requests only from the frontend domain
- The API endpoint must be externally accessible but protected by Azure's built-in security features
- Environment variables must be used for sensitive configuration values

## Logical Architecture

### Component Layers

The application follows a three-tier logical architecture:

**Presentation Layer**: The React frontend application provides the user interface. It consists of reusable components for displaying weather cards, forecast lists, and responsive layouts. The frontend communicates exclusively with the backend API through HTTP requests.

**API Layer**: The .NET backend API serves as the intermediary between the frontend and external data sources. It exposes REST endpoints that the frontend consumes. The API handles request validation, data transformation, and error handling.

**Data Integration Layer**: This layer manages communication with the Open-Meteo API. It includes HTTP clients for fetching weather data and implements caching logic to minimize external API calls.

### Key Components

**Frontend Components**:
- Weather Card Component: Displays weather information for a single day
- Weather Forecast Component: Renders the complete 7-day forecast
- Custom Hooks: useWeather hook manages API calls and state management

**Backend Services**:
- Weather Service: Orchestrates data retrieval and transformation
- Open-Meteo Client: Handles HTTP communication with the Open-Meteo API
- Weather Controller: Exposes REST endpoints for the frontend

**Supporting Infrastructure**:
- Health Check Service: Monitors application status
- Caching Layer: Stores weather data in memory to reduce API calls

## Physical Architecture (Azure)

### Azure Resources

**Azure Container Registry (ACR)**:
- Registry Name: crsharedacrcorchn001
- Purpose: Stores Docker images for both backend and frontend
- Location: West Europe region
- Hosts the latest versions of container images for deployment

**Azure Container Apps (Backend)**:
- Resource Name: como-weather-api
- Purpose: Runs the .NET backend API
- Configuration: 0.5 CPU cores, 1 GB memory
- Replicas: Minimum 1, Maximum 3 with auto-scaling enabled
- Port: 8080 for HTTP traffic
- Health Probe: Checks /api/health endpoint every 10 seconds
- Environment Variables: ASPNETCORE_ENVIRONMENT, OPEN_METEO_BASE_URL, Como coordinates

**Azure Static Web Apps (Frontend)**:
- Resource Name: como-weather-frontend
- Purpose: Hosts the React frontend application
- Build Configuration: Compiles from frontend folder, outputs to dist folder
- CDN Integration: Enabled with 3600-second default TTL and 86400-second max age
- CORS Configuration: Allows requests from the Static Web App domain
- Environment Variables: API base URL for backend communication

### Resource Group and Region

- Resource Group: rg-como-weather
- Region: West Europe
- Subscription: Shared subscription for cost optimization

### Network Configuration

The frontend communicates with the backend through the Container App's external ingress endpoint. The backend communicates with the Open-Meteo API through the public internet. All traffic between Azure services uses Azure's internal network infrastructure.

## Main Flows

### Weather Forecast Display Flow

When a user visits the application, the following sequence occurs:

1. The browser loads the React frontend from Azure Static Web Apps
2. The frontend JavaScript executes and initializes the useWeather hook
3. The hook makes an HTTP GET request to the backend API endpoint /api/weather/como
4. The backend API receives the request and checks its in-memory cache
5. If cached data exists and is fresh, it returns the cached response immediately
6. If no valid cache exists, the backend calls the Open-Meteo API to fetch current weather data
7. The backend transforms the Open-Meteo response into the standardized format
8. The backend stores the response in cache and returns it to the frontend
9. The frontend receives the JSON response and updates the component state
10. React re-renders the weather cards with the new data
11. The user sees the 7-day forecast displayed on the screen

### Health Check Flow

The Azure Container Apps platform continuously monitors the backend health:

1. Every 10 seconds, Azure sends an HTTP GET request to /api/health
2. The backend responds with a JSON object containing status and timestamp
3. If the endpoint responds with HTTP 200 and healthy status, the container is considered healthy
4. If the endpoint fails to respond within 5 seconds or returns unhealthy status, Azure marks the container as unhealthy
5. If a container remains unhealthy, Azure automatically restarts it
6. If traffic increases, Azure scales up to additional replicas based on metrics

### Deployment Flow

The deployment pipeline executes in four stages:

**Build Backend Stage**: NuGet packages are restored, xUnit tests execute, the Docker image is built from the Dockerfile, and the image is pushed to Azure Container Registry.

**Build Frontend Stage**: NPM dependencies are installed, Jest tests execute, the React application is built into static files, and the image is pushed to Azure Container Registry.

**Deploy Backend Stage**: The Container App is updated with the latest image from ACR, environment variables are configured, and the health check endpoint is verified.

**Deploy Frontend Stage**: Static files are deployed to Azure Static Web Apps, the API endpoint environment variable is configured, and CORS settings are verified.

## Security and Scalability

### Security Measures

**API Security**: The backend API does not require authentication for the weather endpoint, as it provides public weather information. However, the API is protected by Azure's network security features and can be restricted to specific IP ranges if needed.

**CORS Configuration**: Cross-Origin Resource Sharing is configured to allow requests only from the Static Web App domain, preventing unauthorized access from other domains.

**Environment Variables**: Sensitive configuration values such as API endpoints and coordinates are stored as environment variables rather than hardcoded in the application code.

**HTTPS Enforcement**: All communication between the frontend and backend uses HTTPS, and the Static Web App automatically redirects HTTP requests to HTTPS.

**Container Security**: Docker images are built from official Microsoft base images and stored in a private Azure Container Registry, ensuring image integrity.

### Scalability Architecture

**Horizontal Scaling**: The backend Container App is configured with auto-scaling that increases the number of replicas from 1 to 3 based on CPU and memory metrics. This allows the application to handle increased traffic without manual intervention.

**Caching Strategy**: In-memory caching reduces the load on the Open-Meteo API and improves response times. The cache is configured with appropriate TTL values to balance freshness and performance.

**CDN Distribution**: The frontend is served through Azure's Content Delivery Network, which caches static files at edge locations worldwide. This reduces latency for users in different geographic regions.

**Stateless Design**: The backend API is designed to be stateless, allowing multiple instances to run independently. This enables seamless scaling and load balancing.

**Load Balancing**: Azure Container Apps automatically distributes incoming requests across multiple replicas using built-in load balancing.

### Performance Optimization

**Response Caching**: API responses are cached with a 3600-second TTL, reducing the number of external API calls and improving response times.

**Static Asset Caching**: Frontend static assets are cached with a maximum age of 86400 seconds, reducing bandwidth usage and improving load times for returning users.

**Efficient Data Transformation**: The backend transforms Open-Meteo data into a compact JSON format that minimizes payload size.

**Resource Allocation**: The backend is allocated 0.5 CPU cores and 1 GB memory, which is sufficient for the expected load while maintaining cost efficiency.

### Monitoring and Observability

**Application Insights**: Logs from the Container App are sent to Azure Application Insights for centralized monitoring and analysis.

**Health Checks**: Regular health checks ensure that unhealthy instances are detected and replaced automatically.

**Metrics Collection**: Azure collects metrics on CPU usage, memory consumption, and request counts to inform auto-scaling decisions.

**Error Tracking**: The backend logs errors and exceptions for troubleshooting and performance analysis.