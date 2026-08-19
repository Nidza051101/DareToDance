FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DareToDance.Api/DareToDance.Api.csproj", "src/DareToDance.Api/"]
COPY ["src/DareToDance.Infrastructure/DareToDance.Infrastructure.csproj", "src/DareToDance.Infrastructure/"]
COPY ["src/DareToDance.Domain/DareToDance.Domain.csproj", "src/DareToDance.Domain/"]
RUN dotnet restore "src/DareToDance.Api/DareToDance.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/DareToDance.Api/DareToDance.Api.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "DareToDance.Api.dll"]
