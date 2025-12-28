# Ecommerce Microservices

Bu repository, **mikroservis mimarisi** ve **event-driven tasarım** prensipleri kullanılarak geliştirilmiş örnek bir e-ticaret backend uygulamasıdır.  
Proje, **yük altında ölçeklenebilirlik**, **güvenilir mesajlaşma**, **idempotent işleme** ve **Docker ile production-benzeri lokal ortam** hedefleriyle tasarlanmıştır.

Sistem üç bounded context’ten oluşur:

- Order
- Stock
- Notification

---

## Projenin Amacı

Bu proje bir hello world mikroservis örneği değildir. Amaç:

- Gerçek hayatta karşılaşılan yük, retry, duplicate message ve eventual consistency problemlerini ele almak
- API ve background worker ayrımının neden kritik olduğunu göstermek
- Outbox, idempotency, saga gibi pattern’leri bilinçli ve servis bazlı uygulamak
- Docker ve Docker Compose ile production’a yakın bir local geliştirme ortamı sunmak

---

## Mimari Genel Bakış

- Event-driven microservices
- RabbitMQ ile asenkron iletişim
- PostgreSQL + EF Core ile transactional güvence
- API ve Worker bileşenlerinin bilinçli ayrımı
- Docker Compose ile infra / app profilleri
- Kubernetes’e birebir taşınabilir yapı

---

## Projede Kullanılan Tüm Pattern ve Özellikler

- Clean Architecture
- Event-Driven Architecture
- Outbox Pattern
- Idempotent Consumers
- Logical Exactly-Once Processing
- At-Least-Once Delivery (RabbitMQ)
- Saga - Choreography
- CQRS + MediatR
- Retry & DLQ
- Polly Retry / Circuit Breaker
- Strategy Pattern
- Unit of Work
- Generic Repository
- EF Core Transactions
- API / Worker ayrımı
- Dockerfile.migrator ile migration yönetimi
- Docker Compose ile container orchestration

Not: Bu pattern’lerin tamamı her serviste kullanılmamıştır.  
Aşağıda her pattern’in hangi serviste kullanıldığı açıkça belirtilmiştir.

---

## Neden API ve Worker Ayrı?

Bu projede API ve Worker bileşenleri bilinçli olarak ayrı container’lar ve ayrı deployment’lar olarak tasarlanmıştır.

### Yük Profilleri Farklıdır

API (HTTP):
- Kısa ömürlü istekler
- Düşük latency beklentisi
- Kullanıcı deneyimi odaklı

Worker (RabbitMQ Consumer):
- Uzun süren işlemler
- Yoğun veritabanı erişimi
- Retry, DLQ ve idempotency maliyeti
- Throughput odaklı çalışma

### Yoğun Yük Senaryosu

Queue’larda binlerce mesaj biriktiğinde:

API pod sayısı sabit kalır  
Worker pod sayısı bağımsız olarak arttırılır

- API: 3 pod
- Worker: 5 -> 10 -> 20 pod

Sonuç:
- API latency etkilenmez
- Backpressure izole edilir
- Sistem öngörülebilir şekilde ölçeklenir

---

## Docker ve Docker Compose

Proje tamamen Docker üzerinde çalıştırılabilir.

### Container Ayrımı

Her servis için ayrı image’lar bulunur:

- Service.Api
- Service.Worker
- Service.Migrator

### Migration Yönetimi

- Her servis için Dockerfile.migrator vardır
- Migration’lar runtime sırasında çalışmaz
- Local, CI/CD veya Kubernetes Job olarak çalıştırılabilir

---

## Docker ile Çalıştırma

Altyapıyı başlatmak için:

docker compose --profile infra up -d

Uygulama servislerini başlatmak için:

docker compose --profile app up -d --build

Sadece belirli bir API değiştiyse:

docker compose up -d --build order-api

---

## Pattern ve Feature Referansları (Servis Bazlı)

### Clean Architecture
Kullanıldığı servisler:
- Order
- Stock
- Notification

---

### Outbox Pattern
Kullanıldığı servisler:
- Order
- Stock
- Notification

---

### CQRS (MediatR)
Kullanıldığı servisler:
- Order

---

### Saga - Choreography
Kullanıldığı servisler:
- Order

---

### Idempotent Consumers
Kullanıldığı servisler:
- Order
- Stock
- Notification

---

### Retry & DLQ
Kullanıldığı servisler:
- Order
- Stock

---

### Polly Retry / Circuit Breaker
Kullanıldığı servisler:
- Notification

---

### Strategy Pattern
Kullanıldığı servisler:
- Notification

---

### API / Worker Ayrımı
Kullanıldığı servisler:
- Order
- Stock
- Notification

---

### Dockerfile.migrator
Kullanıldığı servisler:
- Order
- Stock
- Notification

---

## Son Not

Bu proje:
- Eğitim ve referans amaçlıdır
- Production-ready yaklaşımlar gösterir
- Kubernetes ve cloud ortamlarına kolayca taşınabilir
