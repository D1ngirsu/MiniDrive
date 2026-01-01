FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["MiniDrive.Audit.Api/MiniDrive.Audit.Api.csproj", "MiniDrive.Audit.Api/"]
COPY ["MiniDrive.Audit/MiniDrive.Audit.csproj", "MiniDrive.Audit/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
RUN dotnet restore "MiniDrive.Audit.Api/MiniDrive.Audit.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/MiniDrive.Audit.Api"
RUN dotnet build "MiniDrive.Audit.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Audit.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Audit.Api.dll", "--urls", "http://0.0.0.0:5005"]

