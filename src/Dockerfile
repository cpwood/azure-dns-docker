# Stage 1
FROM --platform:linux/amd64 mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app
# Stage 2
FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "DynamicAzureDns.dll"]