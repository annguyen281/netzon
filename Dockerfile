FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY netzon.api/*.csproj ./netzon.api/
RUN dotnet restore

# copy everything else and build app
COPY netzon.api/. ./netzon.api/
WORKDIR /app/netzon.api
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime
WORKDIR /app
COPY --from=build /app/netzon.api/out ./
ENTRYPOINT ["dotnet", "netzon.api.dll"]