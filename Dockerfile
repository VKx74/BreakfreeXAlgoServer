# Stage 1 
FROM microsoft/aspnetcore-build:2.0 AS builder
WORKDIR /app

COPY . .

RUN dotnet restore .
RUN dotnet publish . -c Release -o /dist

# Stage 2
FROM microsoft/aspnetcore:2.0
WORKDIR /app

COPY --from=builder /dist .


EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "BFT.AlgoService.API.dll"]
