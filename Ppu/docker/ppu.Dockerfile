FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Ppu.csproj ./
RUN dotnet restore Ppu.csproj

COPY . ./
RUN dotnet publish Ppu.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data

COPY --from=build /app/publish ./

EXPOSE 5055

ENTRYPOINT ["dotnet", "Ppu.dll"]
