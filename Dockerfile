# Stage 1 
FROM  microsoft/dotnet:2.2-sdk AS builder
WORKDIR /app

COPY ./NuGet.Config ./NuGet.Config
COPY ./src/AlgoserverAPI ./src/AlgoserverAPI

RUN dotnet restore "./src/AlgoserverAPI/Algoserver.API.csproj" --configfile ./NuGet.Config
RUN	dotnet publish "./src/AlgoserverAPI/Algoserver.API.csproj" -c Release -o /dist

# Stage 2
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app

COPY --from=builder /dist .

EXPOSE 80
ENTRYPOINT ["dotnet", "Algoserver.API.dll"]