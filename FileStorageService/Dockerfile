FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# restore
COPY ["FileStorageService/FileStorageService.csproj", "FileStorageService/"]
RUN dotnet restore "FileStorageService/FileStorageService.csproj"

# build/publish
COPY . .
WORKDIR /src/FileStorageService
RUN dotnet publish "FileStorageService.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# folder for persisted files
RUN mkdir -p /app/StoredFiles

ENV ASPNETCORE_URLS=http://+:5037
EXPOSE 5037

ENTRYPOINT ["dotnet", "FileStorageService.dll"]
