FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app/Server

# Copy csproj and restore as distinct layers
COPY Server/*.csproj /app/Server
COPY Common/*.csproj /app/Common/
RUN dotnet restore

# Copy everything else and build
COPY Server /app/Server
COPY Common /app/Common
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build-env /app/Server/out .
ENTRYPOINT ["dotnet", "WorkNet-Server.dll"]