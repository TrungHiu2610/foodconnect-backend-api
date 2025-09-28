FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FoodConnect.Backend.API/FoodConnect.Backend.API.csproj", "FoodConnect.Backend.API/"]
COPY ["FoodConnect.Backend.Application/FoodConnect.Backend.Application.csproj", "FoodConnect.Backend.Application/"]
COPY ["FoodConnect.Backend.Domain/FoodConnect.Backend.Domain.csproj", "FoodConnect.Backend.Domain/"]
COPY ["FoodConnect.Backend.Infrastructure/FoodConnect.Backend.Infrastructure.csproj", "FoodConnect.Backend.Infrastructure/"]

RUN dotnet restore "FoodConnect.Backend.API/FoodConnect.Backend.API.csproj"
COPY . .
WORKDIR "/src/FoodConnect.Backend.API"
RUN dotnet build "FoodConnect.Backend.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FoodConnect.Backend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080 

ENTRYPOINT ["dotnet", "FoodConnect.Backend.API.dll"]