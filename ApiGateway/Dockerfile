FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# restore
COPY ["ApiGateway/GatewayService.csproj", "ApiGateway/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "ApiGateway/GatewayService.csproj"

# build/publish
COPY . .
WORKDIR /src/ApiGateway
RUN dotnet publish "GatewayService.csproj" -c Release -o /app/publish /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5036
EXPOSE 5036

ENTRYPOINT ["dotnet", "GatewayService.dll"]
