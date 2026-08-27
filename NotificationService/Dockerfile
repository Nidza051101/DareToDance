# Build context je koren repoa (v. Docker/docker-compose.yml: context: ..)
# — mora da vidi proto/ pored NotificationService/, isti razlog kao izmena
# u Backend/Dockerfile. V. artifact "Notification gRPC Flow", deo 5.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["NotificationService/src/NotificationService.Grpc/NotificationService.Grpc.csproj", "NotificationService/src/NotificationService.Grpc/"]
COPY ["NotificationService/src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "NotificationService/src/NotificationService.Infrastructure/"]
COPY ["NotificationService/src/NotificationService.Domain/NotificationService.Domain.csproj", "NotificationService/src/NotificationService.Domain/"]
COPY proto/ proto/
RUN dotnet restore "NotificationService/src/NotificationService.Grpc/NotificationService.Grpc.csproj"

COPY NotificationService/src/ NotificationService/src/
RUN dotnet publish "NotificationService/src/NotificationService.Grpc/NotificationService.Grpc.csproj" -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "NotificationService.Grpc.dll"]
