FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SoftLedger.csproj .
RUN dotnet restore SoftLedger.csproj

COPY . .

RUN dotnet publish SoftLedger.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SoftLedger.dll"]