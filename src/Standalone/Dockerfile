# Use an official .NET 10 SDK as a parent image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set the working directory to /app
WORKDIR /app

# Expose a persistent data mount so data survives container removal
VOLUME ["/app/data"]
COPY data/. ./out/data

# Copy the project files to the container
COPY src/. .

# Remove any bin or obj folders to ensure a clean build
RUN find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} +

# Restore dependencies
RUN dotnet restore Standalone/NeoServer.Server.Standalone.csproj

# Restore dependencies
RUN dotnet restore out/data/extensions/NeoServer.Extensions.csproj

# Build the project
RUN dotnet publish Standalone/NeoServer.Server.Standalone.csproj -c Release -o out

# Use an official .NET 10 runtime as a parent image
FROM mcr.microsoft.com/dotnet/runtime:10.0

# Set the working directory to /app
WORKDIR /app

# Install tzdata and set timezone to America/Sao_Paulo
RUN apt-get update && apt-get install -y --no-install-recommends tzdata && \
    ln -sf /usr/share/zoneinfo/America/Sao_Paulo /etc/localtime && \
    echo "America/Sao_Paulo" > /etc/timezone && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy the published app to the container
COPY --from=build /app/out .

# Set the entrypoint to run the console app
ENTRYPOINT ["dotnet", "NeoServer.Server.Standalone.dll"]