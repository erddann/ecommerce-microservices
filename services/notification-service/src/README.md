# Notification Service

A microservice for handling order notifications via email/SMS.

## Architecture

- **API**: Health checks and optional template management
- **Worker**: Background service consuming RabbitMQ events
- **Application**: Business logic, handlers, context builders
- **Infrastructure**: DB, messaging, repositories

## Setup

1. Clone the repo
2. Run `docker-compose up --build`
3. Services will start with auto-migration enabled

## Environment Variables

- `AUTO_MIGRATE=true`: Enables automatic DB migration on startup (default: false for safety)

## Database

- PostgreSQL with EF Core migrations
- Initial templates seeded via migration

## Messaging

- RabbitMQ for event-driven architecture
- Queues: order.cancelled.queue, order.confirmed.queue

## Development

- Use `dotnet run` for individual services
- For production, set `AUTO_MIGRATE=false` and run migrations separately