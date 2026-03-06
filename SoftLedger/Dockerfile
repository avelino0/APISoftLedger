# Use the .NET SDK image for building the app
FROM ://mcr.microsoft.com AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image for the final, lean application
FROM ://mcr.microsoft.com AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "SoftLedger.dll"]
