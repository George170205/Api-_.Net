# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY WebApplication1/*.csproj ./WebApplication1/
RUN dotnet restore WebApplication1/WebApplication1.csproj

COPY . .
WORKDIR /app/WebApplication1
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/WebApplication1/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
