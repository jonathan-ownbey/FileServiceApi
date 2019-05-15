FROM microsoft/dotnet:2.2-sdk AS base
WORKDIR /app
EXPOSE 51230

# Copy csproj and restore as distinct layers
FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /FileServiceApi
COPY FileServiceApi/FileServiceApi.csproj FileServiceApi/
COPY FileServiceApi.Data/FileServiceApi.Data.csproj FileServiceApi.Data/
COPY FileServiceApi.FileStorers/FileServiceApi.FileStorers.csproj FileServiceApi.FileStorers/
COPY FileServiceApi.Models/FileServiceApi.Models.csproj FileServiceApi.Models/
COPY FileServiceApi.Services/FileServiceApi.Services.csproj FileServiceApi.Services/

RUN dotnet restore -s https://api.nuget.org/v3/index.json -s /FileServiceApi.csproj
# Copy everything else and build
COPY . .

WORKDIR C:Source/Repos/FileServiceApi/FileServiceApi

RUN dotnet build FileServiceApi.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish FileServiceApi.csproj -c Release -o /app


FROM base AS final
WORKDIR /app
COPY --from=publish / .
ENTRYPOINT ["dotnet", "FileServiceApi.dll"]