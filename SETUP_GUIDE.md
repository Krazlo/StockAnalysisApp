# Stock Analysis Application - Setup Guide

This guide will walk you through setting up and running the Stock Analysis Application step by step.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Getting API Keys](#getting-api-keys)
3. [Configuration](#configuration)
4. [Running the Application](#running-the-application)
5. [Testing](#testing)
6. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

1. **.NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation:
     ```bash
     dotnet --version
     ```
     Should output: `8.0.xxx`

2. **Text Editor or IDE** (Optional but recommended)
   - Visual Studio 2022
   - Visual Studio Code
   - JetBrains Rider

## Getting API Keys

### Alpha Vantage API Key (Required)

Alpha Vantage provides free stock market data.

1. Go to: https://www.alphavantage.co/support/#api-key
2. Enter your email address
3. Click "GET FREE API KEY"
4. You'll receive your API key immediately
5. Save this key - you'll need it for configuration

**Note**: The free tier includes:
- 5 API calls per minute
- 500 API calls per day
- This is sufficient for testing and personal use

### Gemini API Key (Required)

Google's Gemini AI provides the analysis capabilities.

1. Go to: https://ai.google.dev/
2. Click "Get API Key" in the top right
3. Sign in with your Google account
4. Click "Create API Key"
5. Select or create a Google Cloud project
6. Copy your API key
7. Save this key - you'll need it for configuration

**Note**: The free tier includes:
- 15 requests per minute
- 1,500 requests per day
- 1 million tokens per minute

## Configuration

### Step 1: Configure Stock Data Service

1. Navigate to the Stock Data Service configuration file:
   ```
   src/StockDataService/appsettings.json
   ```

2. Replace `"demo"` with your Alpha Vantage API key:
   ```json
   {
     "AlphaVantage": {
       "ApiKey": "YOUR_ALPHA_VANTAGE_API_KEY_HERE",
       "BaseUrl": "https://www.alphavantage.co/query"
     }
   }
   ```

3. Save the file

### Step 2: Configure AI Analysis Service

1. Navigate to the AI Analysis Service configuration file:
   ```
   src/AIAnalysisService/appsettings.json
   ```

2. Replace the placeholder with your Gemini API key:
   ```json
   {
     "Gemini": {
       "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
       "BaseUrl": "https://generativelanguage.googleapis.com/v1beta/models",
       "Model": "gemini-2.5-flash"
     }
   }
   ```

3. Save the file

### Step 3: Verify Configuration

All configuration files should now have valid API keys:
- ✅ `src/StockDataService/appsettings.json` - Alpha Vantage API key
- ✅ `src/AIAnalysisService/appsettings.json` - Gemini API key

## Running the Application

### Option 1: Using Multiple Terminals (Recommended for Development)

You'll need **4 separate terminal windows**.

#### Terminal 1: Start API Gateway
```bash
cd StockAnalysisApp/src/ApiGateway
dotnet run
```
Wait for: `Now listening on: http://localhost:5000`

#### Terminal 2: Start Stock Data Service
```bash
cd StockAnalysisApp/src/StockDataService
dotnet run
```
Wait for: `Now listening on: http://localhost:5001`

#### Terminal 3: Start AI Analysis Service
```bash
cd StockAnalysisApp/src/AIAnalysisService
dotnet run
```
Wait for: `Now listening on: http://localhost:5002`

#### Terminal 4: Start UI Application
```bash
cd StockAnalysisApp/src/UIApplication
dotnet run
```
Wait for: `Now listening on: http://localhost:5003`

### Option 2: Using Background Processes (Linux/macOS)

```bash
cd StockAnalysisApp

# Start all services in background
cd src/ApiGateway && dotnet run &
cd ../StockDataService && dotnet run &
cd ../AIAnalysisService && dotnet run &
cd ../UIApplication && dotnet run &

# Wait for all services to start (about 10 seconds)
sleep 10

# Open browser
open http://localhost:5003  # macOS
# or
xdg-open http://localhost:5003  # Linux
```

To stop all services:
```bash
pkill -f "dotnet run"
```

### Option 3: Using PowerShell (Windows)

Create a file named `start-services.ps1`:

```powershell
# Start all services
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/ApiGateway; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/StockDataService; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/AIAnalysisService; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/UIApplication; dotnet run"

# Wait and open browser
Start-Sleep -Seconds 10
Start-Process "http://localhost:5003"
```

Run it:
```powershell
.\start-services.ps1
```

## Testing

### Step 1: Access the Application

Open your web browser and go to:
```
http://localhost:5003
```

You should see the Stock Analysis application homepage.

### Step 2: Test with a Sample Stock

1. **Enter a stock symbol**: `IBM`
   - IBM is used in Alpha Vantage's demo examples
   - Other good test symbols: AAPL, MSFT, GOOGL, TSLA

2. **Enter a prompt**:
   ```
   Analyze the current trend and provide buy/sell recommendations based on technical indicators
   ```

3. **Click "Analyze Stock"**

4. **Wait for results** (may take 10-30 seconds):
   - Stock Data Service fetches data from Alpha Vantage
   - Calculates technical indicators
   - AI Analysis Service sends to Gemini
   - Results are displayed

### Step 3: Test Saving Prompts

1. After analyzing a stock, click "Save This Prompt"
2. Enter a name: `IBM Trend Analysis`
3. Click "Save"
4. The prompt should appear in the "Saved Prompts" sidebar

### Step 4: Test Loading Prompts

1. Click "Load" on a saved prompt
2. The form should populate with the saved data
3. Click "Analyze Stock" to run the analysis again

### Step 5: Verify Services

Check that all services are running:

- **API Gateway**: http://localhost:5000
- **Stock Data Service**: http://localhost:5001/api/stock/IBM
- **AI Analysis Service**: http://localhost:5002/api/analysis/health
- **UI Application**: http://localhost:5003

## Troubleshooting

### Issue: "dotnet: command not found"

**Solution**: Install .NET 8.0 SDK
```bash
# Verify installation
dotnet --version
```

### Issue: "Address already in use" or "Port is already allocated"

**Solution**: Another application is using one of the required ports (5000-5003)

Find and stop the process:
```bash
# Linux/macOS
lsof -ti:5000 | xargs kill
lsof -ti:5001 | xargs kill
lsof -ti:5002 | xargs kill
lsof -ti:5003 | xargs kill

# Windows (PowerShell)
Get-Process -Id (Get-NetTCPConnection -LocalPort 5000).OwningProcess | Stop-Process
```

Or change the ports in the appsettings.json files.

### Issue: "No data returned for symbol"

**Possible causes**:
1. Invalid Alpha Vantage API key
2. Invalid stock symbol
3. API rate limit exceeded
4. Network connectivity issues

**Solution**:
1. Verify your API key in `src/StockDataService/appsettings.json`
2. Try a well-known symbol like `IBM` or `AAPL`
3. Wait a minute if you've made many requests
4. Check internet connection

### Issue: "Gemini API error: 401" or "Unauthorized"

**Solution**: Invalid Gemini API key
1. Verify your API key in `src/AIAnalysisService/appsettings.json`
2. Generate a new API key if needed
3. Ensure there are no extra spaces or quotes

### Issue: "Error fetching stock data: 500"

**Solution**: Check Stock Data Service logs
1. Look at the terminal running StockDataService
2. Check for error messages
3. Verify Alpha Vantage API key is correct
4. Try restarting the service

### Issue: Services start but UI shows errors

**Solution**: Check service connectivity
1. Verify all 4 services are running
2. Check that ports match configuration
3. Verify API Gateway is routing correctly
4. Check browser console for JavaScript errors

### Issue: Saved prompts not persisting

**Solution**: Check file permissions
1. The app saves to: `%APPDATA%\StockAnalysisApp\saved_prompts.json` (Windows)
2. Ensure the application has write permissions
3. Check LocalStorageService logs in UIApplication terminal

### Issue: Build errors

**Solution**: Restore NuGet packages
```bash
cd StockAnalysisApp
dotnet restore
dotnet build
```

## Performance Tips

1. **Caching**: Stock data is cached for 5 minutes by default
   - Reduces API calls
   - Faster subsequent requests for the same symbol

2. **Rate Limits**: Be mindful of API limits
   - Alpha Vantage: 5 calls/minute
   - Gemini: 15 calls/minute
   - Wait between requests if you hit limits

3. **Startup Time**: First request may be slower
   - Services need to warm up
   - Subsequent requests are faster

## Next Steps

Once everything is working:

1. **Explore Different Stocks**: Try various symbols (AAPL, MSFT, GOOGL, TSLA, etc.)
2. **Experiment with Prompts**: Ask different types of questions
3. **Save Favorite Prompts**: Build a library of useful analysis prompts
4. **Review Technical Indicators**: Understand what each indicator means
5. **Compare Analyses**: Run the same prompt on different stocks

## Support Resources

- **Alpha Vantage Documentation**: https://www.alphavantage.co/documentation/
- **Gemini API Documentation**: https://ai.google.dev/gemini-api/docs
- **.NET Documentation**: https://learn.microsoft.com/dotnet/
- **Ocelot Documentation**: https://ocelot.readthedocs.io/

## Security Notes

⚠️ **Important Security Reminders**:

1. **Never commit API keys to version control**
2. **Don't share your API keys publicly**
3. **Use environment variables in production**
4. **Rotate keys periodically**
5. **Monitor API usage for anomalies**

For production deployment, use:
- Environment variables for API keys
- HTTPS for all services
- Authentication and authorization
- Rate limiting
- Logging and monitoring

## Success Checklist

Before considering setup complete, verify:

- ✅ .NET 8.0 SDK installed
- ✅ Alpha Vantage API key obtained and configured
- ✅ Gemini API key obtained and configured
- ✅ Solution builds without errors
- ✅ All 4 services start successfully
- ✅ UI accessible at http://localhost:5003
- ✅ Successfully analyzed at least one stock
- ✅ Saved and loaded at least one prompt

Congratulations! Your Stock Analysis Application is now ready to use! 🎉
