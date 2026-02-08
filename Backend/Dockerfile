# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["OptionsTracker.csproj", "./"]
RUN dotnet restore "OptionsTracker.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "OptionsTracker.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "OptionsTracker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OptionsTracker.dll"]
