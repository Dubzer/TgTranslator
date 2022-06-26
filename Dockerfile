FROM mcr.microsoft.com/dotnet/sdk:6.0.101-alpine3.14 AS build
WORKDIR /source

COPY *.sln .
COPY src/ ./src
RUN dotnet restore -r linux-musl-x64

WORKDIR /source/src
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1-alpine3.14 AS final
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["./TgTranslator"]