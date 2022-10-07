#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["PlayerBankInfoRebuildHash/PlayerBankInfoRebuildHash.csproj", "PlayerBankInfoRebuildHash/"]
COPY ["PaymentRepositories/PaymentRepositories.csproj", "PaymentRepositories/"]
COPY ["Payment.Core/Payment.Core.csproj", "Payment.Core/"]
COPY ["Tgs.Payment.Client/Tgs.Payment.Client.csproj", "Tgs.Payment.Client/"]
RUN dotnet restore "PlayerBankInfoRebuildHash/PlayerBankInfoRebuildHash.csproj"
COPY . .
WORKDIR "/src/PlayerBankInfoRebuildHash"
RUN dotnet build "PlayerBankInfoRebuildHash.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "PlayerBankInfoRebuildHash.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlayerBankInfoRebuildHash.dll"]