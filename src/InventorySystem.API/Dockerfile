# Usar la imagen base de .NET SDK para compilar, publicar y ejecutar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todos los proyectos necesarios
COPY ./InventorySystem.Application ./InventorySystem.Application
COPY ./InventorySystem.Infrastructure ./InventorySystem.Infrastructure
COPY ./InventorySystem.Domain ./InventorySystem.Domain
COPY ./InventorySystem.API ./InventorySystem.API

# Restaurar dependencias
WORKDIR /src/InventorySystem.API
RUN dotnet restore "InventorySystem.API.csproj"

# Compilar el proyecto en modo Release
RUN dotnet publish "InventorySystem.API.csproj" -c Release -o /app/publish

# Usar la imagen base de .NET SDK para ejecutar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Configurar el punto de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "InventorySystem.API.dll"]