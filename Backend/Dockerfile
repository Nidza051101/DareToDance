# Build context je koren repoa (v. Docker/docker-compose.yml: context: ..)
# — potrebno da bi Dockerfile mogao da vidi deljeni proto/ folder pored
# Backend/. V. artifact "Notification gRPC Flow", deo 5.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Backend/src/DareToDance.Api/DareToDance.Api.csproj", "Backend/src/DareToDance.Api/"]
COPY ["Backend/src/DareToDance.Infrastructure/DareToDance.Infrastructure.csproj", "Backend/src/DareToDance.Infrastructure/"]
COPY ["Backend/src/DareToDance.Domain/DareToDance.Domain.csproj", "Backend/src/DareToDance.Domain/"]
COPY proto/ proto/
RUN dotnet restore "Backend/src/DareToDance.Api/DareToDance.Api.csproj"

COPY Backend/src/ Backend/src/
RUN dotnet publish "Backend/src/DareToDance.Api/DareToDance.Api.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "DareToDance.Api.dll"]
