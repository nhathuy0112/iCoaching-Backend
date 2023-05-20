FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY *.sln .
COPY src/Core/*.csproj ./src/Core/
COPY src/Infrastructure/*.csproj ./src/Infrastructure/
COPY src/API/*.csproj ./src/API/
COPY tests/UnitTest/*.csproj ./tests/UnitTest/
RUN dotnet restore

COPY src/Core/. ./src/Core/
COPY src/Infrastructure/. ./src/Infrastructure/
COPY src/API/. ./src/API/
COPY tests/UnitTest ./tests/UnitTest/
WORKDIR /source/src/API
RUN dotnet publish -c release -o /published --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ENV TZ="Asia/Ho_Chi_Minh"
WORKDIR /app
COPY --from=build /published ./
ENTRYPOINT ["dotnet", "API.dll"]