FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ARG DLL
ARG TARGETPLATFORM
COPY publish/$TARGETPLATFORM /app
WORKDIR /app
ENV ENV_DLL=$DLL
ENTRYPOINT dotnet $ENV_DLL