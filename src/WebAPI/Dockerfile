ARG ASPNET_VERSION="9.0"
ARG SDK_VERSION="9.0"
ARG BASE_ADRESS="mcr.microsoft.com/dotnet"

FROM $BASE_ADRESS/aspnet:$ASPNET_VERSION AS base
WORKDIR /app

EXPOSE 80

FROM $BASE_ADRESS/sdk:$SDK_VERSION AS build

WORKDIR /src

COPY *.csproj ./WebAPI

RUN dotnet restore -v m ./WebAPI

WORKDIR /src/WebAPI
RUN dotnet build NeoServer.Web.API.csproj -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish NeoServer.Web.API.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeoServer.Web.API.dll"]