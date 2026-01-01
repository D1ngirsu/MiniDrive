FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Gateway.Api/MiniDrive.Gateway.Api.csproj", "MiniDrive.Gateway.Api/"]
RUN dotnet restore "MiniDrive.Gateway.Api/MiniDrive.Gateway.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Gateway.Api"
RUN dotnet build "MiniDrive.Gateway.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Gateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Gateway.Api.dll", "--urls", "http://0.0.0.0:5000"]

