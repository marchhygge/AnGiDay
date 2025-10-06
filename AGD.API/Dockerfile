# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and publish the API project
COPY . .
# Publish will perform restore for the specified project
RUN dotnet publish "AGD.API/AGD.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Optional: reduce image size by removing unused locales (Debian-based runtime already trimmed)
# Copy published output
COPY --from=build /app/publish .

# Listen on port 80 inside container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Start the API
ENTRYPOINT ["dotnet", "AGD.API.dll"]