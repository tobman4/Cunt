FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /build

COPY ./src /build
RUN dotnet restore Cunt

RUN dotnet build -c Release -o /app Cunt

FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=build /app /app
CMD [ "dotnet", "Cunt.dll" ]
