#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["PlayerBankInfoRebuildHash/PlayerBankInfoRebuildHash.csproj", "PlayerBankInfoRebuildHash/"]
COPY ["PaymentRepositories/PaymentRepositories.csproj", "PaymentRepositories/"]
COPY ["Payment.Core/Payment.Core.csproj", "Payment.Core/"]
COPY ["Tgs.Payment.Client/Tgs.Payment.Client.csproj", "Tgs.Payment.Client/"]
RUN dotnet restore "./PlayerBankInfoRebuildHash/./PlayerBankInfoRebuildHash.csproj"
COPY . .
WORKDIR "/src/PlayerBankInfoRebuildHash"
RUN dotnet build "./PlayerBankInfoRebuildHash.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PlayerBankInfoRebuildHash.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlayerBankInfoRebuildHash.dll"]