FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Files.Api/MiniDrive.Files.Api.csproj", "MiniDrive.Files.Api/"]
COPY ["MiniDrive.Files/MiniDrive.Files.csproj", "MiniDrive.Files/"]
COPY ["MiniDrive.Storage/MiniDrive.Storage.csproj", "MiniDrive.Storage/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
COPY ["MiniDrive.Clients/MiniDrive.Clients.csproj", "MiniDrive.Clients/"]
RUN dotnet restore "MiniDrive.Files.Api/MiniDrive.Files.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Files.Api"
RUN dotnet build "MiniDrive.Files.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Files.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Files.Api.dll", "--urls", "http://0.0.0.0:5002"]

