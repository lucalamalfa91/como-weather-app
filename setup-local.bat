@echo off
REM Como Weather Forecast - Local Setup Script (Windows)
REM This script sets up the development environment for local testing

echo.
echo 🌤️  Como Weather Forecast - Local Setup
echo ========================================
echo.

REM Check prerequisites
echo 📋 Checking prerequisites...

where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Node.js not found. Please install Node.js 20+ from https://nodejs.org
    exit /b 1
)

where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com
    exit /b 1
)

for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i

echo ✅ Node.js %NODE_VERSION%
echo ✅ .NET %DOTNET_VERSION%
echo.

REM Setup backend
echo 🔧 Setting up backend...
cd backend

if not exist ".env" (
    echo Creating backend .env file...
    (
        echo ASPNETCORE_ENVIRONMENT=Development
        echo OPEN_METEO_BASE_URL=https://api.open-meteo.com/v1
        echo COMO_LATITUDE=45.8081
        echo COMO_LONGITUDE=9.0852
    ) > .env
)

echo Restoring .NET packages...
dotnet restore

echo ✅ Backend setup complete!
echo.

REM Setup frontend
echo 🎨 Setting up frontend...
cd ..\frontend

if not exist ".env" (
    echo Creating frontend .env file...
    (
        echo VITE_API_BASE_URL=http://localhost:8080
    ) > .env
)

echo Installing npm packages...
call npm install

echo ✅ Frontend setup complete!
echo.

REM Done
echo 🎉 Setup complete!
echo.
echo To start the application:
echo.
echo 1. Start the backend (in terminal 1):
echo    cd backend ^&^& dotnet run
echo.
echo 2. Start the frontend (in terminal 2):
echo    cd frontend ^&^& npm run dev
echo.
echo 3. Open your browser:
echo    http://localhost:5173
echo.
echo Happy coding! 🚀

cd ..
pause
