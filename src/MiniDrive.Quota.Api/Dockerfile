FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Quota.Api/MiniDrive.Quota.Api.csproj", "MiniDrive.Quota.Api/"]
COPY ["MiniDrive.Quota/MiniDrive.Quota.csproj", "MiniDrive.Quota/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
RUN dotnet restore "MiniDrive.Quota.Api/MiniDrive.Quota.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Quota.Api"
RUN dotnet build "MiniDrive.Quota.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Quota.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Quota.Api.dll", "--urls", "http://0.0.0.0:5004"]

