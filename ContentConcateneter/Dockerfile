FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ContentConcateneter/ContentConcateneter.csproj", "ContentConcateneter/"]
RUN dotnet restore "ContentConcateneter/ContentConcateneter.csproj"
COPY . .
WORKDIR "/src/ContentConcateneter"
RUN dotnet build "ContentConcateneter.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContentConcateneter.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContentConcateneter.dll"]
