# Use an official .NET 9 SDK as a parent image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory to /app
WORKDIR /app

# Copy the project files to the container
COPY src/. .

# Restore dependencies
RUN dotnet restore WebAPI/NeoServer.Web.API.csproj

# Build the project
RUN dotnet publish WebAPI/NeoServer.Web.API.csproj -c Release -o out

# Use an official .NET 9 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Set the working directory to /app
WORKDIR /app

# Copy the published app to the container
COPY --from=build /app/out .

# Set the entrypoint to run the console app
ENTRYPOINT ["dotnet", "NeoServer.Web.API.dll"]