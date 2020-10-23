FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ARG DLL
ARG TARGETPLATFORM
WORKDIR /app
COPY publish/$TARGETPLATFORM /app .
ENTRYPOINT ["dotnet", $DLL]