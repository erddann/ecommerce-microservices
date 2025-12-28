# 🛒 Ecommerce-microservices

🚀 A production-grade, event-driven e-commerce backend built with **.NET**, **RabbitMQ**, **PostgreSQL**, and **Docker**.

This repository demonstrates real-world backend architecture patterns used in high-throughput distributed systems.

---

## ✨ Key Features & Patterns

✔ Event-Driven Architecture  
✔ Outbox Pattern  
✔ Idempotent Consumers  
✔ Worker-based Message Processing  
✔ Retry & Dead Letter Queue (DLQ)  
✔ Structured Logging  
✔ Choreography-based Saga  
✔ Independent API / Worker scaling  
✔ Database-per-service  
✔ Containerized & Kubernetes-friendly design  

---

## 🧩 Services

### 🛒 Order Service
- Accepts order creation requests
- Persists orders and domain events
- Writes integration events to Outbox table
- Publishes events via background workers
- Reacts to stock processing results

### 📦 Stock Service
- Consumes OrderCreated events via worker
- Performs stock deduction
- Implements retry (max 3) and DLQ
- Publishes StockProcessCompleted / StockProcessFailed events
- Ensures idempotent processing

### 🔔 Notification Service
- Consumes OrderConfirmed / OrderCancelled events
- Builds notifications using DB-driven templates
- Fetches additional customer data if required
- Uses fallback data when external service is unavailable
- Implements resilience patterns (retry, circuit breaker)
- Logs notification results

---

## 🏗️ Internal Architecture (4 Layers)

Each service follows a strict **4-layer architecture**.

### 1️⃣ API Layer 🌐
- HTTP Controllers
- Request / response mapping
- Delegates all logic to Application layer
- No business logic

### 2️⃣ Application Layer 🧠
- Business rules & use cases
- Application services & handlers
- Workflow orchestration
- Interface definitions for Infrastructure

### 3️⃣ Infrastructure Layer 🔌
- EF Core & database access
- Entity configurations
- Generic repositories
- Outbox & ProcessedMessages persistence
- RabbitMQ & external service integrations

### 4️⃣ Worker Layer ⚙️
- Background jobs
- RabbitMQ consumers
- Retry & DLQ handling
- Idempotency checks
- Event publishing

---

## 🔀 API vs Worker Separation

### API
- Handles HTTP traffic only
- Stateless
- Never consumes messages

### Worker
- Consumes RabbitMQ messages
- Executes background jobs
- Handles retries, DLQ, idempotency
- Publishes new events

This separation allows **independent scaling** under heavy load.

---

## 🧠 Core Patterns

### 📦 Outbox Pattern
- Domain events are saved in the same DB transaction
- Background workers publish events asynchronously
- Prevents message loss and inconsistent state

### ♻️ Idempotent Consumers
- Each consumed message is recorded in `ProcessedMessages`
- Duplicate deliveries are ignored
- Guarantees exactly-once business behavior

### 🔄 Choreography-Based Saga
- No central orchestrator
- Services react to events
- Flow: Order → Stock → Order → Notification

### 🚨 Retry & Dead Letter Queue
- Stock processing retries up to 3 times
- Failed messages are routed to DLQ
- Failure events are still published

---

## 🧾 Logging

Structured logging is applied across all layers.
Logs are designed for debugging, monitoring, and production troubleshooting.

## 🗄️ Database Migration Strategy

Migrations are executed **before** application containers start.

### Flow
1. Infrastructure starts (PostgreSQL, RabbitMQ)
2. Migrator containers apply EF Core migrations and exit
3. API & Worker containers start

APIs and Workers never run migrations.

---

## 🐳 Docker Architecture

Each service is deployed using three container types.

### 🌐 API Container
- Hosts HTTP endpoints
- Stateless
- Scales by HTTP traffic

### ⚙️ Worker Container
- Runs background jobs
- Consumes RabbitMQ messages
- Handles retry, DLQ, idempotency
- Scales by queue depth

### 🗄️ Migrator Container
- Applies database migrations
- Runs once and exits

---

## 🧩 Docker Compose Profiles

### infra
- PostgreSQL
- RabbitMQ

### app
- Migrators
- APIs
- Workers

### Startup Order

```bash
docker compose --profile infra up -d
docker compose --profile app up -d
```

---

## 📈 Scalability

- API scales based on HTTP load
- Worker scales based on queue depth
- Message spikes do not affect API availability

Designed to be Kubernetes-ready.

---

## 🚀 Possible Improvements

- FluentValidation
- Dedicated DLQ consumers
- Distributed tracing (OpenTelemetry)
- Metrics (Prometheus / Grafana)
- Authentication & Authorization

---
