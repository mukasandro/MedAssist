# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MedAssist.sln ./
COPY src/MedAssist.Api/MedAssist.Api.csproj src/MedAssist.Api/
COPY src/MedAssist.Application/MedAssist.Application.csproj src/MedAssist.Application/
COPY src/MedAssist.Domain/MedAssist.Domain.csproj src/MedAssist.Domain/
COPY src/MedAssist.Infrastructure/MedAssist.Infrastructure.csproj src/MedAssist.Infrastructure/
COPY src/MedAssist.LlmGateway.Api/MedAssist.LlmGateway.Api.csproj src/MedAssist.LlmGateway.Api/
RUN dotnet restore

COPY . .
RUN dotnet publish src/MedAssist.Api/MedAssist.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedAssist.Api.dll"]
