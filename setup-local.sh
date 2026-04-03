#!/bin/bash

# Como Weather Forecast - Local Setup Script
# This script sets up the development environment for local testing

set -e

echo "🌤️  Como Weather Forecast - Local Setup"
echo "========================================"
echo ""

# Check prerequisites
echo "📋 Checking prerequisites..."

if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js 20+ from https://nodejs.org"
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com"
    exit 1
fi

echo "✅ Node.js $(node --version)"
echo "✅ .NET $(dotnet --version)"
echo ""

# Setup backend
echo "🔧 Setting up backend..."
cd backend

if [ ! -f ".env" ]; then
    echo "Creating backend .env file..."
    cat > .env << EOF
ASPNETCORE_ENVIRONMENT=Development
OPEN_METEO_BASE_URL=https://api.open-meteo.com/v1
COMO_LATITUDE=45.8081
COMO_LONGITUDE=9.0852
EOF
fi

echo "Restoring .NET packages..."
dotnet restore

echo "✅ Backend setup complete!"
echo ""

# Setup frontend
echo "🎨 Setting up frontend..."
cd ../frontend

if [ ! -f ".env" ]; then
    echo "Creating frontend .env file..."
    cat > .env << EOF
VITE_API_BASE_URL=http://localhost:8080
EOF
fi

echo "Installing npm packages..."
npm install

echo "✅ Frontend setup complete!"
echo ""

# Done
echo "🎉 Setup complete!"
echo ""
echo "To start the application:"
echo ""
echo "1. Start the backend (in terminal 1):"
echo "   cd backend && dotnet run"
echo ""
echo "2. Start the frontend (in terminal 2):"
echo "   cd frontend && npm run dev"
echo ""
echo "3. Open your browser:"
echo "   http://localhost:5173"
echo ""
echo "Happy coding! 🚀"
