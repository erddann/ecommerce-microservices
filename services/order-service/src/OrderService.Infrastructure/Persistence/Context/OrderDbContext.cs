using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Common;
using OrderService.Infrastructure.Entities;
using OrderService.Infrastructure.Outbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Infrastructure.Persistence.Context
{
    public class OrderDbContext : DbContext
    {
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
        public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }
        public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
        {
        //    var domainEvents = ChangeTracker
        //        .Entries<AggregateRoot>()
        //        .SelectMany(e => e.Entity.DomainEvents)
        //        .ToList();

        //    var outboxMessages = domainEvents
        //.Select(e => new OutboxMessage(e))
        //.ToList();

        //    foreach (var message in outboxMessages)
        //    {
        //        Set<OutboxMessage>().Add(message);
        //    }

            var result = await base.SaveChangesAsync(cancellationToken);

            foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            {
                entry.Entity.ClearDomainEvents();
            }

            return result;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<DomainEvent>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        }
    }
}
