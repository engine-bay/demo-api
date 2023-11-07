FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS base

ARG VERSION=0.0.0

WORKDIR /tmp/build

# copy csproj and restore as distinct layers
COPY ./EngineBay.DemoApi/EngineBay.DemoApi.csproj ./EngineBay.DemoApi/EngineBay.DemoApi.csproj
RUN dotnet restore ./EngineBay.DemoApi/EngineBay.DemoApi.csproj

# copy everything else and build
COPY ./EngineBay.DemoApi ./EngineBay.DemoApi
COPY .editorconfig .editorconfig
RUN dotnet publish ./EngineBay.DemoApi/EngineBay.DemoApi.csproj --nologo --runtime linux-musl-x64 --self-contained --configuration Release -o /tmp/build/out /p:Version=$VERSION

# build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine
RUN apk add icu-libs
COPY --from=base /tmp/build/out /usr/local/sbin

EXPOSE 5051
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN mkdir /seed-data
ENTRYPOINT ["EngineBay.DemoApi"]