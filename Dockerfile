# Sử dụng .NET 10 SDK để build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file .csproj và restore các dependencies
COPY ["BackendService/BackendService.csproj", "BackendService/"]
RUN dotnet restore "BackendService/BackendService.csproj"

# Copy toàn bộ code và build
COPY . .
RUN dotnet build "BackendService/BackendService.csproj" -c Release -o /app/build
RUN dotnet publish "BackendService/BackendService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Image runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port mặc định của Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "BackendService.dll"]
