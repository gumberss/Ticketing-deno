FROM mcr.microsoft.com/dotnet/core/sdk:3.1.301-alpine3.12 as build-env
WORKDIR /app
EXPOSE 80

COPY Products.sln ./

COPY Tests/Tests.csproj Tests/
COPY Domain/Domain.csproj Domain/
COPY Infra/Infra.csproj Infra/
COPY WebApi/WebApi.csproj WebApi/

RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5-alpine3.12
WORKDIR /app
COPY --from=build-env /app/out .

RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "WebApi.dll"]


