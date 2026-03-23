FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PrepWise.Client/PrepWise.Client.csproj", "PrepWise.Client/"]
RUN dotnet restore "PrepWise.Client/PrepWise.Client.csproj"
COPY PrepWise.Client/ PrepWise.Client/
WORKDIR "/src/PrepWise.Client"
RUN dotnet publish "PrepWise.Client.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY PrepWise.Client/nginx.conf /etc/nginx/conf.d/default.conf
