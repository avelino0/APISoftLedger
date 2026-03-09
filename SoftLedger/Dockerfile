# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SoftLedger/SoftLedger.csproj SoftLedger/
RUN dotnet restore SoftLedger/SoftLedger.csproj

COPY . .

RUN dotnet publish SoftLedger/SoftLedger.csproj -c Release -o /app/publish

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SoftLedger.dll"]