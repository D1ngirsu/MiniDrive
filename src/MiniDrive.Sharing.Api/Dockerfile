FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/MiniDrive.Sharing.Api/MiniDrive.Sharing.Api.csproj", "src/MiniDrive.Sharing.Api/"]
COPY ["src/MiniDrive.Sharing/MiniDrive.Sharing.csproj", "src/MiniDrive.Sharing/"]
COPY ["src/MiniDrive.Common/MiniDrive.Common.csproj", "src/MiniDrive.Common/"]
COPY ["src/MiniDrive.Clients/MiniDrive.Clients.csproj", "src/MiniDrive.Clients/"]
RUN dotnet restore "src/MiniDrive.Sharing.Api/MiniDrive.Sharing.Api.csproj"
COPY . .
WORKDIR "/src/src/MiniDrive.Sharing.Api"
RUN dotnet build "MiniDrive.Sharing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Sharing.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Sharing.Api.dll"]
