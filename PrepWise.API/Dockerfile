FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PrepWise.API/PrepWise.API.csproj", "PrepWise.API/"]
RUN dotnet restore "PrepWise.API/PrepWise.API.csproj"
COPY PrepWise.API/ PrepWise.API/
WORKDIR "/src/PrepWise.API"
RUN dotnet build "PrepWise.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PrepWise.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PrepWise.API.dll"]
