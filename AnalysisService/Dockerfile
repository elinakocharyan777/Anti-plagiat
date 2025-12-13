FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# restore 
COPY ["AnalysisService/AnalysisService.csproj", "AnalysisService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "AnalysisService/AnalysisService.csproj"

# build/publish
COPY . .
WORKDIR /src/AnalysisService
RUN dotnet publish "AnalysisService.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# приложение лежит тут
WORKDIR /app
COPY --from=build /app/publish .

# данные (SQLite) будем хранить отдельно, чтобы удобно монтировать volume
RUN mkdir -p /data
WORKDIR /data

ENV ASPNETCORE_URLS=http://+:5027
EXPOSE 5027

# запуск из /data, чтобы analysis.db создавался внутри /data
ENTRYPOINT ["dotnet", "/app/AnalysisService.dll"]
