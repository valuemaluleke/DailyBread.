# 1. Use the official .NET SDK 9.0 to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["DailyBread.csproj", "./"]
RUN dotnet restore

# Copy the rest of the source code
COPY . .
# Build and publish the app to a folder
RUN dotnet publish -c Release -o /app/publish

# 2. Use the runtime image 9.0 to run the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Tell .NET to listen on port 8080 (Required for Render)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Start the app!
ENTRYPOINT ["dotnet", "DailyBread.dll"]