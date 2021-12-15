#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BuildConfiguration=Release
RUN --mount=type=secret,id=nuget,dst=/pat cp /pat ~/.nuget/NuGet/NuGet.Config
RUN dotnet nuget remove source 'Microsoft Visual Studio Offline Packages' || true
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
COPY ["PaymentRepositories/PaymentRepositories.csproj", "PaymentRepositories/"]
COPY ["Payment.Core/Payment.Core.csproj", "Payment.Core/"]
COPY ["Tgs.Payment.Client/Tgs.Payment.Client.csproj", "Tgs.Payment.Client/"]
RUN dotnet restore "PaymentService/PaymentService.csproj"
COPY . .
WORKDIR "/src/PaymentService"
RUN dotnet build "PaymentService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "PaymentService.csproj" -c $BuildConfiguration -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]