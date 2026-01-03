FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

WORKDIR /src

COPY ["src/AIAnalysisService/AIAnalysisService.csproj", "AIAnalysisService/"]
RUN dotnet restore "AIAnalysisService/AIAnalysisService.csproj"

COPY ["src/AIAnalysisService/", "AIAnalysisService/"]

WORKDIR /src/AIAnalysisService
RUN dotnet build "AIAnalysisService.csproj" -c Release -o /app/build
RUN dotnet build "./AIAnalysisService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AIAnalysisService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIAnalysisService.dll"]