FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["MiniDrive.Sharing.Api/MiniDrive.Sharing.Api.csproj", "MiniDrive.Sharing.Api/"]
COPY ["MiniDrive.Sharing/MiniDrive.Sharing.csproj", "MiniDrive.Sharing/"]
COPY ["MiniDrive.Common/MiniDrive.Common.csproj", "MiniDrive.Common/"]
COPY ["MiniDrive.Clients/MiniDrive.Clients.csproj", "MiniDrive.Clients/"]
RUN dotnet restore "MiniDrive.Sharing.Api/MiniDrive.Sharing.Api.csproj"
COPY . .
WORKDIR "/src/MiniDrive.Sharing.Api"
RUN dotnet build "MiniDrive.Sharing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MiniDrive.Sharing.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MiniDrive.Sharing.Api.dll"]
