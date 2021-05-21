FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PoolAlerter.csproj", "./"]
RUN dotnet restore "PoolAlerter.csproj"
COPY . .
WORKDIR "/src/PoolAlerter"
RUN dotnet build "PoolAlerter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PoolAlerter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PoolAlerter.dll"]
