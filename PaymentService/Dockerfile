FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
COPY ["Payment.Core.Cache/Payment.Core.Cache.csproj", "Payment.Core.Cache/"]
COPY ["Payment.Core/Payment.Core.csproj", "Payment.Core/"]
COPY ["Tgs.Payment.Client/Tgs.Payment.Client.csproj", "Tgs.Payment.Client/"]
COPY ["PaymentRepositories/PaymentRepositories.csproj", "PaymentRepositories/"]
COPY ["WithdrawProcessors/WithdrawProcessors.csproj", "WithdrawProcessors/"]
RUN dotnet restore "./PaymentService/./PaymentService.csproj"
COPY . .
WORKDIR "/src/PaymentService"
RUN dotnet build "./PaymentService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PaymentService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]