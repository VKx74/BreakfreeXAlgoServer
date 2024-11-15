# Stage 1 
FROM  mcr.microsoft.com/dotnet/sdk:3.1 AS builder
WORKDIR /app

COPY ./NuGet.Config ./NuGet.Config
COPY ./src/AlgoserverAPI ./src/AlgoserverAPI

RUN dotnet restore "./src/AlgoserverAPI/Algoserver.API.csproj" --configfile ./NuGet.Config
RUN	dotnet publish "./src/AlgoserverAPI/Algoserver.API.csproj" -c Release -o /dist

# Stage 2
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app

COPY --from=builder /dist .

EXPOSE 80
ENTRYPOINT ["dotnet", "Algoserver.API.dll"]