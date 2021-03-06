﻿FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY FIRSTShares/*.csproj ./FIRSTShares/
COPY FIRSTShares.API/*.csproj ./FIRSTShares.API/

RUN dotnet restore

# copy everything else and build app
COPY FIRSTShares/. ./FIRSTShares/
COPY FIRSTShares.API/. ./FIRSTShares.API/

WORKDIR /app
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "FIRSTShares.API.dll"]