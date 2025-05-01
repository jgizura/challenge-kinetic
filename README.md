# Desarrollador Backend .NET
## Desafío Técnico
### Sistema de Notificaciones de Inventario

## Contexto
Desarrollar un sistema simple para la gestión de actualizaciones de inventario entre dos microservicios, usando RabbitMQ como middleware de mensajería.

# Inventory System Solution

## Prerequisites

Ensure you have the following installed on your system:

- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)

## Setting Up the Solution

1. Clone the repository to your local machine.
2. Navigate to the root directory of the project where the `docker-compose.yml` file is located.

## Running the Solution

1. Open a terminal in the root directory.
2. Run the following command to build and start the services:

   ```bash
   docker-compose up --build
   ```

   This will:
   - Build and start the `InventorySystem.API` service on port `5000`.
   - Build and start the `InventarySystem.Consumer` service.
   - Start a RabbitMQ instance on ports `5672` (for messaging) and `15672` (for management UI).
   - Start a SQL Server instance on port `1433`.

3. Wait for all services to initialize. The API will be accessible at `http://localhost:5000`.

## Arquitectura del Sistema

El sistema está compuesto por los siguientes componentes principales:

```
+--------------------+       +--------------------+       +--------------------+
|                    |       |                    |       |                    |
| InventorySystem.API|       | RabbitMQ           | <----> | InventarySystem.   |
| (Servicio API)     |       | (Mensajería)       |       | Consumer           |
|                    |       |                    |       | (Servicio Consumidor)|
+--------------------+       +--------------------+       +--------------------+
         |                      ^                          |
         v                      |                          v
+--------------------+           |               +--------------------+
|                    |           |               |                    |
| Application Layer  | <---------+               | Application Layer  |
| (DTOs, Features,   |                           | (DTOs, Features,   |
|  Mappings)         |                           |  Mappings)         |
|                    |                           |                    |
+--------------------+                           +--------------------+
         |                                              |
         v                                              v
+--------------------+                           +--------------------+
|                    |                           |                    |
| Infrastructure     |                           | Infrastructure     |
| (Data, Repositorios|                           | (Data, Repositorios|
|  Migrations)       |                           |  Migrations)       |
|                    |                           |                    |
+--------------------+                           +--------------------+
         |                                              |
         v                                              v
+--------------------+                           +--------------------+
|                    |                           |                    |
| SQL Server         |                           | SQL Server         |
| (Base de Datos)    |                           | (Base de Datos)    |
|                    |                           |                    |
+--------------------+                           +--------------------+
```

### Descripción de los Componentes

1. **InventorySystem.API**: Servicio API que expone endpoints para interactuar con el sistema de inventario. Se conecta a la capa de Application.
2. **Application Layer**: Contiene la lógica de aplicación, como DTOs, características y mapeos. Se conecta a la capa de Infrastructure y también inicializa RabbitMQ.
3. **Infrastructure Layer**: Implementa la lógica de acceso a datos, repositorios y migraciones. Se conecta a SQL Server.
4. **SQL Server**: Base de datos que almacena la información del inventario y los productos.
5. **RabbitMQ**: Sistema de mensajería que interactúa con la capa de Application y el servicio Consumer.
6. **InventarySystem.Consumer**: Servicio que consume mensajes de RabbitMQ para procesar eventos relacionados con el inventario. Interactúa con la capa de Application y la capa de Infrastructure.

## Environment Variables

The following environment variables are used in the solution:

- `ASPNETCORE_ENVIRONMENT`: Set to `Development`.
- `DEFAULT_CONNECTION`: Connection string for the SQL Server database.
- `RABBITMQ_HOST`: Hostname for RabbitMQ.

These are pre-configured in the `docker-compose.yml` file.

## Stopping the Solution

To stop the services, run:

```bash
docker-compose down
```

## Additional Notes

- The `InventorySystem.API` and `InventarySystem.Consumer` services are built using .NET 8.0.
- The RabbitMQ management UI can be accessed at `http://localhost:15672` (default username: `guest`, password: `guest`).
- Ensure that ports `5000`, `5672`, `15672`, and `1433` are not in use by other applications.