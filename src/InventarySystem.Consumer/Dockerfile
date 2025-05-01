# Usar la imagen base de .NET SDK para compilar y publicar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todos los proyectos necesarios
COPY ./InventorySystem.Application ./InventorySystem.Application
COPY ./InventorySystem.Infrastructure ./InventorySystem.Infrastructure
COPY ./InventorySystem.Domain ./InventorySystem.Domain
COPY ./InventarySystem.Consumer ./InventarySystem.Consumer

# Restaurar dependencias
WORKDIR /src/InventarySystem.Consumer
RUN dotnet restore "InventarySystem.Consumer.csproj"

# Compilar el proyecto en modo Release
RUN dotnet publish "InventarySystem.Consumer.csproj" -c Release -o /app/publish

# Usar la imagen base de .NET para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Solucionar problemas de apt-get e instalar netcat
RUN apt-get update && apt-get install -y --no-install-recommends netcat-openbsd && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Configurar el punto de entrada
ENTRYPOINT ["dotnet", "InventorySystem.Consumer.dll"]