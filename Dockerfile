# syntax=docker/dockerfile:1

# ---- Build stage: .NET SDK ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so `dotnet restore` is cached until a .csproj changes.
COPY src/EBatterySwap.Domain/EBatterySwap.Domain.csproj src/EBatterySwap.Domain/
COPY src/EBatterySwap.Application/EBatterySwap.Application.csproj src/EBatterySwap.Application/
COPY src/EBatterySwap.Infrastructure/EBatterySwap.Infrastructure.csproj src/EBatterySwap.Infrastructure/
COPY src/EBatterySwap.API/EBatterySwap.API.csproj src/EBatterySwap.API/
RUN dotnet restore src/EBatterySwap.API/EBatterySwap.API.csproj

COPY src/ src/
RUN dotnet publish src/EBatterySwap.API/EBatterySwap.API.csproj \
    -c Release -o /app/publish --no-restore

# ---- Runtime stage: ASP.NET ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run as the non-root user built into the .NET images.
USER $APP_UID
ENTRYPOINT ["dotnet", "EBatterySwap.API.dll"]
