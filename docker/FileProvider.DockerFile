FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app/FileProvider

# Copy csproj and restore as distinct layers
COPY FileProvider/*.csproj /app/FileProvider
COPY Common/*.csproj /app/Common/
RUN dotnet restore

# Copy everything else and build
COPY FileProvider /app/FileProvider
COPY Common /app/Common
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
WORKDIR /app
COPY --from=build-env /app/FileProvider/out .
ENTRYPOINT ["dotnet", "WorkNet-FileProvider.dll"]