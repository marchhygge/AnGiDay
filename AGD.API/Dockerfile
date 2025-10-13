# Mẫu Dockerfile cho ASP.NET Core (multi-stage)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# copy csproj(s) and restore as distinct layers
COPY *.sln .
COPY AnGiDay/*.csproj ./AnGiDay/
# nếu có thêm project, copy tương tự
RUN dotnet restore

# copy everything and build
COPY . .
WORKDIR /src/AnGiDay
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# đảm bảo ứng dụng bind tới port Fly gán (đọc PORT trong Program.cs là an toàn nhất)
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "AnGiDay.dll"]