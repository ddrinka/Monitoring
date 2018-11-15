FROM microsoft/dotnet:2.1-sdk AS base
WORKDIR /source
COPY . .
RUN dotnet publish src/Monitoring.Nest.App/Monitoring.Nest.App.csproj --output /app/ --configuration Release

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app

COPY --from=base /app .

ENTRYPOINT ["dotnet", "Monitoring.Nest.App.dll" ]
