#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["PlayerBankInfoReindex/PlayerBankInfoReindex.csproj", "PlayerBankInfoReindex/"]
COPY ["PaymentRepositories/PaymentRepositories.csproj", "PaymentRepositories/"]
COPY ["Payment.Core/Payment.Core.csproj", "Payment.Core/"]
COPY ["Tgs.Payment.Client/Tgs.Payment.Client.csproj", "Tgs.Payment.Client/"]
RUN dotnet restore "PlayerBankInfoReindex/PlayerBankInfoReindex.csproj"
COPY . .
WORKDIR "/src/PlayerBankInfoReindex"
RUN dotnet build "PlayerBankInfoReindex.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlayerBankInfoReindex.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlayerBankInfoReindex.dll"]