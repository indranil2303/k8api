# -------- build --------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# -------- runtime --------
FROM mcr.microsoft.com/dotnet/aspnet:9.0

RUN adduser --disabled-password --gecos "" appuser
USER appuser

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "k8api.dll"]