# 1. Use the official .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DailyBread.csproj", "./"]
RUN dotnet restore

# Copy the rest of the source code
COPY . .
# Build and publish the app to a folder
RUN dotnet publish -c Release -o /app/publish

# 2. Use the runtime image to run the app (smaller and faster)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Tell .NET to listen on port 8080 (Required for Render)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Start the app!
ENTRYPOINT ["dotnet", "DailyBread.dll"]