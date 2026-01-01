FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Folders.Api/MiniDrive.Folders.Api.csproj", "MiniDrive.Folders.Api/"]
COPY ["MiniDrive.Folders/MiniDrive.Folders.csproj", "MiniDrive.Folders/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
COPY ["MiniDrive.Clients/MiniDrive.Clients.csproj", "MiniDrive.Clients/"]
RUN dotnet restore "MiniDrive.Folders.Api/MiniDrive.Folders.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Folders.Api"
RUN dotnet build "MiniDrive.Folders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Folders.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Folders.Api.dll", "--urls", "http://0.0.0.0:5003"]

