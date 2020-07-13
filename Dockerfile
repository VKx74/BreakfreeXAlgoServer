# Stage 1 
FROM  microsoft/dotnet:2.2-sdk AS builder
WORKDIR /app

COPY ./NuGet.Config ./NuGet.Config

COPY ./src/TwelvedataAPI ./src/TwelvedataAPI
COPY ./src/Twelvedata.Client ./src/Twelvedata.Client

RUN dotnet restore --configfile ./NuGet.Config ./src/TwelvedataAPI/Twelvedata.API.csproj
RUN dotnet publish ./src/TwelvedataAPI/Twelvedata.API.csproj -c Release -o /dist


# Stage 2
FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app

COPY --from=builder /dist .

EXPOSE 80
ENTRYPOINT ["dotnet", "Twelvedata.API.dll"]