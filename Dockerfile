FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

COPY *.sln .
COPY src/ ./src
RUN dotnet restore -r linux-musl-x64

WORKDIR /source/src
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["./TgTranslator"]