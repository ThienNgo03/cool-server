FROM mcr.microsoft.com/dotnet/sdk:9.0 as build
WORKDIR /app
EXPOSE 80

COPY cool-server.sln cool-server.sln
COPY src/Journal/Journal.csproj src/Journal/Journal.csproj

RUN dotnet restore cool-server.sln

COPY src/Journal src/Journal
WORKDIR /app/src/Journal
RUN dotnet publish -c Release -o /app/src/Journal/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/src/Journal/out .
ENTRYPOINT [ "dotnet", "Journal.dll" ]