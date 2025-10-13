# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files & restore (copying everything for simplicity)
COPY . .

# Publish API project (project file is in this folder)
RUN dotnet publish "AGD.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Listen on port 80 inside container
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Start the API
ENTRYPOINT ["dotnet", "AGD.API.dll"]