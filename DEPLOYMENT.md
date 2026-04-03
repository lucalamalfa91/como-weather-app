# 🚀 Como Weather Forecast - Deployment Guide

This guide will help you deploy the **Como Weather Forecast** application using:
- **Vercel** (Frontend - 100% FREE)
- **Railway** (Backend - $5 FREE credit/month)

---

## 📋 Prerequisites

1. **GitHub Account** - To store your code and run GitHub Actions
2. **Vercel Account** - Sign up at [vercel.com](https://vercel.com) (FREE)
3. **Railway Account** - Sign up at [railway.app](https://railway.app) ($5 FREE credit/month)

---

## 🎯 Step 1: Setup Railway (Backend)

### 1.1 Create Railway Project

1. Go to [railway.app](https://railway.app) and log in
2. Click **"New Project"**
3. Select **"Deploy from GitHub repo"**
4. Select this repository
5. Railway will auto-detect the Dockerfile in `generated_app/backend/`

### 1.2 Configure Environment Variables

In your Railway project dashboard, add these environment variables:

```env
ASPNETCORE_ENVIRONMENT=Production
OPEN_METEO_BASE_URL=https://api.open-meteo.com/v1
COMO_LATITUDE=45.8081
COMO_LONGITUDE=9.0852
PORT=8080
```

### 1.3 Get Railway Backend URL

1. After deployment, Railway will provide a public URL like: `https://your-app.railway.app`
2. **Copy this URL** - you'll need it for Vercel configuration

### 1.4 Get Railway Token for CI/CD

1. Go to **Account Settings** → **Tokens**
2. Click **"Create New Token"**
3. Copy the token - you'll add it to GitHub Secrets

---

## 🌐 Step 2: Setup Vercel (Frontend)

### 2.1 Import Project to Vercel

1. Go to [vercel.com/dashboard](https://vercel.com/dashboard)
2. Click **"Add New..."** → **"Project"**
3. Import your GitHub repository
4. Set **Root Directory** to: `generated_app/frontend`
5. Vercel will auto-detect the configuration from `vercel.json`

### 2.2 Configure Environment Variables

In Vercel project settings → **Environment Variables**, add:

```env
VITE_API_BASE_URL=https://your-backend.railway.app
```

Replace `https://your-backend.railway.app` with your actual Railway backend URL from Step 1.3.

### 2.3 Get Vercel Tokens for CI/CD

1. Go to **Settings** → **Tokens** → Create a new token
2. Go to your project **Settings** → copy **Project ID**
3. Go to **Account Settings** → copy **Team/Org ID**

You'll need:
- `VERCEL_TOKEN`
- `VERCEL_PROJECT_ID`
- `VERCEL_ORG_ID`

---

## 🔐 Step 3: Configure GitHub Secrets

Add these secrets to your GitHub repository:

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"** and add:

| Secret Name | Description | Where to Find |
|-------------|-------------|---------------|
| `RAILWAY_TOKEN` | Railway API token | Railway Account Settings → Tokens |
| `RAILWAY_BACKEND_URL` | Your Railway backend URL | Railway project dashboard (e.g., `https://your-app.railway.app`) |
| `VERCEL_TOKEN` | Vercel API token | Vercel Account Settings → Tokens |
| `VERCEL_PROJECT_ID` | Vercel project ID | Vercel Project Settings |
| `VERCEL_ORG_ID` | Vercel organization ID | Vercel Account Settings |

---

## 🚦 Step 4: Deploy Automatically

Once you've configured all secrets:

1. Push any commit to the `main` branch:
   ```bash
   git add .
   git commit -m "Deploy Como Weather Forecast"
   git push origin main
   ```

2. GitHub Actions will automatically:
   - Run tests for frontend and backend
   - Deploy backend to Railway
   - Deploy frontend to Vercel

3. Check the **Actions** tab in GitHub to monitor deployment progress

---

## 🧪 Step 5: Verify Deployment

### Check Backend (Railway)

Visit your Railway backend URL:
```
https://your-backend.railway.app/api/health
```

You should see:
```json
{
  "status": "healthy",
  "timestamp": "2026-04-04T..."
}
```

### Check Weather API

Visit:
```
https://your-backend.railway.app/api/weather/como
```

You should see weather data in JSON format.

### Check Frontend (Vercel)

Visit your Vercel URL (e.g., `https://your-app.vercel.app`)

You should see the Como Weather Forecast with 7-day forecast displayed.

---

## 💰 Cost Breakdown

| Service | Cost | Limits |
|---------|------|--------|
| **Vercel** | **$0/month** | Unlimited bandwidth, 100GB/month |
| **Railway** | **$5 FREE credit/month** | ~500 execution hours (plenty for this app) |
| **Total** | **$0/month** | (Railway credit is FREE) |

---

## 🐛 Troubleshooting

### Frontend can't connect to backend

1. Check that `VITE_API_BASE_URL` in Vercel matches your Railway URL
2. Verify CORS is enabled in backend (already configured in `Program.cs`)
3. Check Railway backend is running: visit `/api/health` endpoint

### Railway deployment fails

1. Check Railway logs for errors
2. Verify Dockerfile is in `generated_app/backend/` directory
3. Ensure all environment variables are set in Railway dashboard

### Vercel deployment fails

1. Check build logs in Vercel dashboard
2. Verify `vercel.json` is in `generated_app/frontend/` directory
3. Run `npm run build` locally to test build process

### GitHub Actions fails

1. Check that all GitHub Secrets are correctly set
2. Verify secret names match exactly (case-sensitive)
3. Check Actions logs for specific error messages

---

## 🔄 Manual Deployment (Alternative)

If you prefer to deploy manually without GitHub Actions:

### Deploy Backend to Railway

```bash
cd generated_app/backend
npm install -g @railway/cli
railway login
railway init
railway up
```

### Deploy Frontend to Vercel

```bash
cd generated_app/frontend
npm install -g vercel
vercel login
vercel --prod
```

---

## 📚 Additional Resources

- [Railway Documentation](https://docs.railway.app)
- [Vercel Documentation](https://vercel.com/docs)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)

---

## 🎉 Success!

Your Como Weather Forecast application is now live:
- **Frontend (Vercel)**: `https://your-app.vercel.app`
- **Backend (Railway)**: `https://your-backend.railway.app`

Enjoy your **FREE** weather forecast app! 🌤️
