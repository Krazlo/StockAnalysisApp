# Stock Analysis Application - API Documentation

This document provides detailed information about the REST APIs in the Stock Analysis Application.

## Table of Contents

1. [Overview](#overview)
2. [API Gateway](#api-gateway)
3. [Stock Data Service](#stock-data-service)
4. [AI Analysis Service](#ai-analysis-service)
5. [Error Handling](#error-handling)
6. [Rate Limits](#rate-limits)

## Overview

The application uses a microservice architecture with an Ocelot API Gateway that routes requests to backend services.

**Base URLs**:
- API Gateway: `http://localhost:5000`
- Stock Data Service: `http://localhost:5001`
- AI Analysis Service: `http://localhost:5002`
- UI Application: `http://localhost:5003`

## API Gateway

The API Gateway (Ocelot) provides a single entry point for all client requests.

### Routes

#### Get Stock Data
```
GET /stock/{symbol}
```

Routes to: `Stock Data Service -> GET /api/stock/{symbol}`

**Description**: Fetches complete stock data with technical indicators

**Parameters**:
- `symbol` (path, required): Stock ticker symbol (e.g., AAPL, MSFT)

**Example Request**:
```bash
curl http://localhost:5000/stock/AAPL
```

**Example Response**:
```json
{
  "symbol": "AAPL",
  "currentData": {
    "symbol": "AAPL",
    "date": "2025-11-04T00:00:00",
    "open": 225.50,
    "high": 228.75,
    "low": 224.30,
    "close": 227.50,
    "volume": 52000000
  },
  "historicalData": [...],
  "indicators": {
    "sma_20": 223.45,
    "sma_50": 220.30,
    "sma_200": 195.80,
    "ema_12": 225.60,
    "ema_26": 222.40,
    "rsi_14": 58.30,
    "macd_Line": 3.20,
    "macd_Signal": 2.85,
    "macd_Histogram": 0.35,
    "bollingerUpper": 230.50,
    "bollingerMiddle": 223.45,
    "bollingerLower": 216.40,
    "averageVolume_20": 48500000,
    "volumeChangePercent": 7.22,
    "currentPrice": 227.50,
    "dayChangePercent": 1.25,
    "week52High": 237.50,
    "week52Low": 164.08,
    "priceVsSMA50": "Above",
    "priceVsSMA200": "Above"
  },
  "retrievedAt": "2025-11-04T10:30:00Z"
}
```

#### Analyze Stock
```
POST /analysis/analyze
```

Routes to: `AI Analysis Service -> POST /api/analysis/analyze`

**Description**: Analyzes stock with AI based on technical indicators and user prompt

**Request Body**:
```json
{
  "prompt": "Analyze the current trend and provide recommendations",
  "symbol": "AAPL",
  "stockData": {
    "symbol": "AAPL",
    "currentPrice": 227.50,
    "date": "2025-11-04T00:00:00",
    "indicators": {
      "sma_20": 223.45,
      "sma_50": 220.30,
      ...
    }
  }
}
```

**Example Request**:
```bash
curl -X POST http://localhost:5000/analysis/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Analyze the trend",
    "symbol": "AAPL",
    "stockData": {...}
  }'
```

**Example Response**:
```json
{
  "symbol": "AAPL",
  "analysis": "Based on the technical indicators provided...",
  "timestamp": "2025-11-04T10:30:00Z",
  "success": true,
  "errorMessage": null
}
```

## Stock Data Service

Direct access to the Stock Data Service (bypassing the gateway).

### Endpoints

#### Get Stock Data with Indicators
```
GET /api/stock/{symbol}
```

**Description**: Fetches stock data from Alpha Vantage and calculates all technical indicators

**Parameters**:
- `symbol` (path, required): Stock ticker symbol

**Response**: Same as Gateway's `/stock/{symbol}` endpoint

**Status Codes**:
- `200 OK`: Success
- `400 Bad Request`: Invalid symbol
- `404 Not Found`: Symbol not found
- `500 Internal Server Error`: API error

**Example**:
```bash
curl http://localhost:5001/api/stock/MSFT
```

#### Get Stock Indicators Only
```
GET /api/stock/{symbol}/indicators
```

**Description**: Returns only the indicators and current price (lighter response)

**Parameters**:
- `symbol` (path, required): Stock ticker symbol

**Example Response**:
```json
{
  "symbol": "MSFT",
  "currentPrice": 380.50,
  "indicators": {
    "sma_20": 375.30,
    "sma_50": 368.20,
    ...
  },
  "retrievedAt": "2025-11-04T10:30:00Z"
}
```

**Example**:
```bash
curl http://localhost:5001/api/stock/MSFT/indicators
```

### Technical Indicators Explained

#### Moving Averages
- **SMA_20**: 20-day Simple Moving Average
- **SMA_50**: 50-day Simple Moving Average
- **SMA_200**: 200-day Simple Moving Average
- **EMA_12**: 12-day Exponential Moving Average
- **EMA_26**: 26-day Exponential Moving Average

#### Momentum Indicators
- **RSI_14**: 14-day Relative Strength Index (0-100)
  - < 30: Oversold
  - > 70: Overbought
- **MACD_Line**: MACD Line (EMA12 - EMA26)
- **MACD_Signal**: Signal Line (9-day EMA of MACD)
- **MACD_Histogram**: Histogram (MACD - Signal)

#### Volatility Indicators
- **BollingerUpper**: Upper Bollinger Band (SMA + 2σ)
- **BollingerMiddle**: Middle Bollinger Band (20-day SMA)
- **BollingerLower**: Lower Bollinger Band (SMA - 2σ)

#### Volume Metrics
- **AverageVolume_20**: 20-day average volume
- **VolumeChangePercent**: Current volume vs average (%)

#### Price Metrics
- **CurrentPrice**: Latest closing price
- **DayChangePercent**: Day-over-day price change (%)
- **Week52High**: Highest price in last 52 weeks
- **Week52Low**: Lowest price in last 52 weeks
- **PriceVsSMA50**: "Above" or "Below" 50-day SMA
- **PriceVsSMA200**: "Above" or "Below" 200-day SMA

### Caching

The Stock Data Service implements in-memory caching:
- **Duration**: 5 minutes (configurable)
- **Key**: Stock symbol
- **Purpose**: Reduce API calls to Alpha Vantage

To change cache duration, edit `appsettings.json`:
```json
{
  "Caching": {
    "StockDataCacheDurationMinutes": 5
  }
}
```

## AI Analysis Service

Direct access to the AI Analysis Service (bypassing the gateway).

### Endpoints

#### Analyze Stock
```
POST /api/analysis/analyze
```

**Description**: Sends stock data and user prompt to Gemini AI for analysis

**Request Body**:
```json
{
  "prompt": "string (required)",
  "symbol": "string (required)",
  "stockData": {
    "symbol": "string",
    "currentPrice": "decimal",
    "date": "datetime",
    "indicators": {
      "sma_20": "decimal",
      "sma_50": "decimal",
      ...
    }
  }
}
```

**Response**:
```json
{
  "symbol": "string",
  "analysis": "string",
  "timestamp": "datetime",
  "success": "boolean",
  "errorMessage": "string (nullable)"
}
```

**Status Codes**:
- `200 OK`: Analysis successful
- `400 Bad Request`: Invalid request (missing prompt, symbol, or data)
- `500 Internal Server Error`: AI service error

**Example**:
```bash
curl -X POST http://localhost:5002/api/analysis/analyze \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "Provide a comprehensive analysis",
    "symbol": "GOOGL",
    "stockData": {
      "symbol": "GOOGL",
      "currentPrice": 140.50,
      "date": "2025-11-04T00:00:00",
      "indicators": {
        "sma_20": 138.30,
        "rsi_14": 62.5
      }
    }
  }'
```

#### Health Check
```
GET /api/analysis/health
```

**Description**: Check if the service is running

**Response**:
```json
{
  "status": "healthy",
  "service": "AI Analysis Service"
}
```

**Example**:
```bash
curl http://localhost:5002/api/analysis/health
```

### Prompt Engineering

The service automatically formats your prompt with all technical data:

**Input Prompt**:
```
Analyze the current trend
```

**Formatted Prompt Sent to Gemini**:
```
User Prompt: Analyze the current trend

Stock Analysis Request for: AAPL

Current Stock Data:
- Symbol: AAPL
- Current Price: $227.50
- Date: 2025-11-04

Technical Indicators:

Moving Averages:
- SMA 20-day: $223.45
- SMA 50-day: $220.30
- SMA 200-day: $195.80
- EMA 12-day: $225.60
- EMA 26-day: $222.40

Momentum Indicators:
- RSI (14-day): 58.30
- MACD Line: 3.20
- MACD Signal: 2.85
- MACD Histogram: 0.35

Bollinger Bands:
- Upper Band: $230.50
- Middle Band: $223.45
- Lower Band: $216.40

Volume Analysis:
- Average Volume (20-day): 48,500,000
- Volume Change: 7.22%

Price Metrics:
- Day Change: 1.25%
- 52-Week High: $237.50
- 52-Week Low: $164.08
- Price vs SMA 50: Above
- Price vs SMA 200: Above

Please provide a comprehensive stock analysis based on the above data and the user's prompt.
```

## Error Handling

### Standard Error Response

All services return errors in a consistent format:

```json
{
  "error": "Error message description"
}
```

### Common Error Codes

#### 400 Bad Request
**Causes**:
- Missing required parameters
- Invalid stock symbol format
- Malformed JSON

**Example**:
```json
{
  "error": "Symbol is required"
}
```

#### 404 Not Found
**Causes**:
- Stock symbol not found in Alpha Vantage
- Invalid endpoint

**Example**:
```json
{
  "error": "No data returned for symbol: INVALID"
}
```

#### 500 Internal Server Error
**Causes**:
- External API errors (Alpha Vantage, Gemini)
- Service configuration issues
- Network problems

**Example**:
```json
{
  "error": "An error occurred while fetching stock data"
}
```

### Service-Specific Errors

#### Stock Data Service

**Alpha Vantage API Errors**:
```json
{
  "error": "No data returned for symbol: XYZ"
}
```

**Configuration Errors**:
```json
{
  "error": "Alpha Vantage API key is not configured"
}
```

#### AI Analysis Service

**Gemini API Errors**:
```json
{
  "error": "Gemini API error: 401"
}
```

**Missing Data Errors**:
```json
{
  "error": "Stock data is required"
}
```

## Rate Limits

### Alpha Vantage (Stock Data Service)

**Free Tier Limits**:
- 5 API calls per minute
- 500 API calls per day

**Mitigation**:
- Caching enabled (5-minute default)
- Same symbol requests within 5 minutes use cache

**Rate Limit Response**:
```json
{
  "Note": "Thank you for using Alpha Vantage! Our standard API call frequency is 5 calls per minute..."
}
```

### Gemini API (AI Analysis Service)

**Free Tier Limits**:
- 15 requests per minute
- 1,500 requests per day
- 1 million tokens per minute

**Rate Limit Response**:
```json
{
  "error": "Gemini API error: 429"
}
```

### Best Practices

1. **Use Caching**: Stock data is cached automatically
2. **Batch Requests**: Analyze multiple aspects in one prompt
3. **Monitor Usage**: Check API dashboards regularly
4. **Handle Errors**: Implement retry logic with exponential backoff
5. **Upgrade if Needed**: Consider paid tiers for production use

## Testing with cURL

### Complete Workflow Example

```bash
# 1. Get stock data
curl http://localhost:5000/stock/TSLA > stock_data.json

# 2. Analyze with AI (using saved data)
curl -X POST http://localhost:5000/analysis/analyze \
  -H "Content-Type: application/json" \
  -d @analysis_request.json

# 3. Check service health
curl http://localhost:5002/api/analysis/health
```

### Sample Request Files

**analysis_request.json**:
```json
{
  "prompt": "Analyze the momentum and provide short-term outlook",
  "symbol": "TSLA",
  "stockData": {
    "symbol": "TSLA",
    "currentPrice": 242.50,
    "date": "2025-11-04T00:00:00",
    "indicators": {
      "sma_20": 238.30,
      "sma_50": 235.60,
      "rsi_14": 65.40,
      "macd_Line": 4.50,
      "macd_Signal": 3.20
    }
  }
}
```

## Postman Collection

You can import these endpoints into Postman:

1. Create a new collection: "Stock Analysis API"
2. Add environment variables:
   - `gateway_url`: http://localhost:5000
   - `stock_service_url`: http://localhost:5001
   - `ai_service_url`: http://localhost:5002

3. Add requests:
   - GET `{{gateway_url}}/stock/AAPL`
   - POST `{{gateway_url}}/analysis/analyze`
   - GET `{{stock_service_url}}/api/stock/MSFT`
   - GET `{{ai_service_url}}/api/analysis/health`

## Swagger/OpenAPI

Each service has Swagger UI enabled in development mode:

- **Stock Data Service**: http://localhost:5001/swagger
- **AI Analysis Service**: http://localhost:5002/swagger

Access these URLs to:
- View all endpoints
- Test API calls
- See request/response schemas
- Download OpenAPI specifications

## Security Considerations

### API Keys
- Never expose API keys in responses
- Store keys in configuration files (not in code)
- Use environment variables in production

### CORS
- Currently configured to allow all origins (development)
- Restrict origins in production

### HTTPS
- Use HTTPS in production
- Configure SSL certificates
- Redirect HTTP to HTTPS

### Authentication
- Consider adding authentication for production
- Implement API key authentication
- Use OAuth 2.0 for user authentication

### Rate Limiting
- Implement rate limiting in API Gateway
- Protect against abuse
- Monitor usage patterns

## Monitoring and Logging

All services log to console in development:

**Log Levels**:
- Information: Normal operations
- Warning: Potential issues
- Error: Failures and exceptions

**Key Log Messages**:
- "Fetching stock data for symbol: {Symbol}"
- "Successfully fetched and cached stock data"
- "Sending request to Gemini API"
- "Successfully received analysis from Gemini"

## Support

For API-related issues:
- Check service logs in terminal
- Verify API keys are configured
- Test endpoints with cURL or Postman
- Review error messages carefully
- Consult external API documentation:
  - Alpha Vantage: https://www.alphavantage.co/documentation/
  - Gemini API: https://ai.google.dev/gemini-api/docs
