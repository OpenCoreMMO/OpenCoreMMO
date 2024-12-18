ARG ASPNET_VERSION="9.0"
ARG SDK_VERSION="9.0"
ARG BASE_ADRESS="mcr.microsoft.com/dotnet"

FROM $BASE_ADRESS/aspnet:$ASPNET_VERSION AS base
WORKDIR /app

EXPOSE 80

FROM $BASE_ADRESS/sdk:$SDK_VERSION AS build

WORKDIR /src

COPY *.csproj ./Standalone

RUN dotnet restore -v m ./Standalone

WORKDIR /src/Standalone
RUN dotnet build -c Release --no-restore -v m -o /app/build

FROM build AS publish
RUN dotnet publish -c Release --no-restore -v m -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeoServer.Server.Standalone.dll"]