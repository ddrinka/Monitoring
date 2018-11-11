FROM microsoft/dotnet:2.1-sdk AS base
WORKDIR /source
COPY . .
RUN dotnet publish src/Monitoring.MotorolaCableModem.App/Monitoring.MotorolaCableModem.App.csproj --output /app/ --configuration Release

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app

COPY --from=base /app .

ENTRYPOINT ["dotnet", "Monitoring.MotorolaCableModem.App.dll" ]
