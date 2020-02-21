FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app/Agent

# Copy csproj and restore as distinct layers
COPY Agent/*.csproj /app/Agent
COPY Common/*.csproj /app/Common/
RUN dotnet restore

# Copy everything else and build
COPY Agent /app/Agent
COPY Common /app/Common
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build-env /app/Agent/out .
ENTRYPOINT ["dotnet", "WorkNet-Agent.dll"]