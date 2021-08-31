# Stage 1 
FROM  mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder
WORKDIR /app

COPY ./NuGet.Config ./NuGet.Config
COPY ./src/AlgoserverAPI ./src/AlgoserverAPI

RUN dotnet restore "./src/AlgoserverAPI/Algoserver.API.csproj" --configfile ./NuGet.Config
RUN	dotnet publish "./src/AlgoserverAPI/Algoserver.API.csproj" -c Release -o /dist

# Stage 2
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app

COPY --from=builder /dist .

EXPOSE 80
ENTRYPOINT ["dotnet", "Algoserver.API.dll"]