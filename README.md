# Stock Analysis Application with AI

A C# microservice application that provides intelligent stock analysis using real-time market data and AI-powered insights through Google's Gemini AI.

## Features

- **Real-time Stock Data**: Fetches live stock data from EODHD API
- **Technical Indicators**: Automatically calculates 15+ technical indicators including:
  - Simple Moving Averages (SMA 20, 50, 200)
  - Exponential Moving Averages (EMA 12, 26)
  - Relative Strength Index (RSI)
  - MACD (Moving Average Convergence Divergence)
  - Bollinger Bands
  - Volume Analysis
  - Price Metrics (52-week high/low, day change %)
- **AI-Powered Analysis**: Uses Gemini AI to provide comprehensive stock analysis based on technical indicators and user prompts
- **Microservice Architecture**: Built with REST APIs and Ocelot API Gateway
- **Local Storage**: Save and reuse your favorite prompts and symbols
- **User-Friendly UI**: Clean web interface built with ASP.NET Core MVC and Bootstrap

## Architecture

The application consists of four main components:

1. **API Gateway** (Port 5000) - Ocelot-based gateway that routes requests
2. **Stock Data Service** (Port 5001) - Fetches stock data and calculates indicators
3. **AI Analysis Service** (Port 5002) - Integrates with Gemini AI for analysis
4. **UI Application** (Port 5003) - Web interface for user interaction

```
User → UI (5003) → API Gateway (5000) → Stock Service (5001) → Alpha Vantage API
                                      → AI Service (5002) → Gemini API
```

## Prerequisites

- .NET 8.0 SDK
- EODHD API Key
- Gemini API Key (free at https://ai.google.dev/)

## Installation

### 1. Clone or Extract the Project

```bash
cd /path/to/StockAnalysisApp
```

### 2. Configure API Keys (in your .env file locally or using Github Secrets)

EODHD_API_KEY="YOUR_EODHD_KEY"
GEMINI_API_KEY="YOUR_GEMINI_KEY"

### 3. Build the Solution

```bash
dotnet build
```

## Running the Application

1. Open Docker Dekstop
2. In your IDE with the solution opened run:
   ````
   docker-compose up -d --build
   ```` 
### 4. Access the Application

Open your web browser and navigate to:
```
http://localhost:5003
```

## Usage

### Analyzing a Stock

1. Enter a stock symbol (e.g., AAPL, MSFT, GOOGL, TSLA)
2. Enter an exchange (e.g., CO, US)
3. Write your analysis prompt (e.g., "Analyze the current trend and provide buy/sell recommendations")
4. Click "Analyze Stock"
5. Wait for the AI to process the data and provide analysis

### Saving Prompts

1. After entering a prompt and symbol, click "Save This Prompt"
2. Give it a memorable name
3. Click "Save"
4. Your saved prompts will appear in the sidebar

### Loading Saved Prompts

1. Find your saved prompt in the sidebar
2. Click "Load" to populate the form with that prompt and symbol
3. Click "Analyze Stock" to run the analysis

### Deleting Saved Prompts

1. Find the prompt you want to delete
2. Click "Delete"
3. Confirm the deletion

## API Endpoints

### API Gateway (http://localhost:5000)

- `GET /stock/{symbol}` - Get stock data with indicators
- `POST /analysis/analyze` - Analyze stock with AI

### Stock Data Service (http://localhost:5001)

- `GET /api/stock/{symbol}` - Get full stock data with indicators
- `GET /api/stock/{symbol}/indicators` - Get indicators only

### AI Analysis Service (http://localhost:5002)

- `POST /api/analysis/analyze` - Analyze stock with custom prompt
- `GET /api/analysis/health` - Health check

## Project Structure

```
StockAnalysisApp/
├── src/
│   ├── ApiGateway/              # Ocelot API Gateway
│   │   ├── ocelot.json          # Ocelot routing configuration
│   │   └── Program.cs
│   │
│   ├── StockDataService/        # Stock data and indicators
│   │   ├── Controllers/
│   │   │   └── StockController.cs
│   │   ├── Models/
│   │   │   ├── StockData.cs
│   │   │   ├── StockIndicators.cs
│   │   │   └── AlphaVantageResponse.cs
│   │   ├── Services/
│   │   │   ├── StockDataServiceImpl.cs
│   │   │   └── IndicatorCalculator.cs
│   │   └── appsettings.json
│   │
│   ├── AIAnalysisService/       # Gemini AI integration
│   │   ├── Controllers/
│   │   │   └── AnalysisController.cs
│   │   ├── Models/
│   │   │   ├── AnalysisRequest.cs
│   │   │   ├── AnalysisResponse.cs
│   │   │   └── GeminiApiModels.cs
│   │   ├── Services/
│   │   │   └── GeminiService.cs
│   │   └── appsettings.json
│   │
│   └── UIApplication/           # Web UI
│       ├── Controllers/
│       │   └── HomeController.cs
│       ├── Models/
│       │   ├── AnalysisViewModel.cs
│       │   └── SavedPrompt.cs
│       ├── Services/
│       │   ├── ApiService.cs
│       │   └── LocalStorageService.cs
│       ├── Views/
│       │   └── Home/
│       │       └── Index.cshtml
│       └── appsettings.json
│
├── StockAnalysisApp.sln
└── README.md
```

## Technical Details

### Stock Data Service

**Data Source**: EODHD API
- Implements caching (5-minute default) to reduce API calls

**Technical Indicators Calculated**:
- **Moving Averages**: SMA (20, 50, 200 day), EMA (12, 26 day)
- **Momentum**: RSI (14 day), MACD with signal line and histogram
- **Volatility**: Bollinger Bands (20 day, 2 std dev)
- **Volume**: 20-day average, volume change percentage
- **Price Metrics**: Day change %, 52-week high/low, price vs SMA positions

### AI Analysis Service

**AI Model**: Google Gemini 2.5 Flash
- Fast and cost-efficient
- 1 million token context window
- Excellent for financial analysis

**Analysis Process**:
1. Receives user prompt and stock data with indicators
2. Formats comprehensive prompt with all technical data
3. Sends to Gemini API
4. Returns AI-generated analysis


## API Rate Limits

### EODHD (Free Tier)
- 20 API calls per day
- Caching implemented to minimize calls

### Gemini API (Free Tier)
- 15 requests per minute
- 1,500 requests per day
- 1 million tokens per minute

## Troubleshooting

### "No data returned for symbol"
- Verify the stock symbol is correct
- Check if the market is open (Alpha Vantage may have delays)
- Ensure your Alpha Vantage API key is valid

### "Gemini API error"
- Verify your Gemini API key is correct
- Check if you've exceeded rate limits
- Ensure internet connectivity

### Services won't start
- Check if ports 5000-5003 are available
- Verify .NET 8.0 SDK is installed: `dotnet --version`
- Check firewall settings

### Build errors
- Run `dotnet restore` to restore NuGet packages
- Ensure all dependencies are installed
- Check .NET SDK version compatibility

## Technologies Used

- **Backend**: ASP.NET Core 8.0, C#
- **API Gateway**: Ocelot 24.0
- **Frontend**: ASP.NET Core MVC, Bootstrap 5
- **APIs**: EODHD, Google Gemini AI
- **Serialization**: System.Text.Json
- **Caching**: IMemoryCache

## License

This project is provided as-is for educational and personal use.

## Support

For issues or questions:
1. Check the Troubleshooting section
2. Review API documentation:
   - EODHD: https://eodhd.com/financial-apis/
   - Gemini API: https://ai.google.dev/gemini-api/docs

## Acknowledgments

- EODHD for providing free stock market data
- Google for the Gemini AI API
- Microsoft for the .NET platform and Ocelot gateway