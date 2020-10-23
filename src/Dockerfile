FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ARG DLL
ARG TARGETPLATFORM
COPY publish/$TARGETPLATFORM /app
WORKDIR /app
ENTRYPOINT ["dotnet", "$DLL"]