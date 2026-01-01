FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Identity.Api/MiniDrive.Identity.Api.csproj", "MiniDrive.Identity.Api/"]
COPY ["MiniDrive.Identity/MiniDrive.Identity.csproj", "MiniDrive.Identity/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
RUN dotnet restore "MiniDrive.Identity.Api/MiniDrive.Identity.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Identity.Api"
RUN dotnet build "MiniDrive.Identity.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Identity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Identity.Api.dll", "--urls", "http://0.0.0.0:5001"]

