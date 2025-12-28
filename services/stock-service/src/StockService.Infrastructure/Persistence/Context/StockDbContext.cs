using Microsoft.EntityFrameworkCore;
using StockService.Domain.Entities;
using StockService.Infrastructure.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockService.Infrastructure.Persistence.Context
{
    public sealed class StockDbContext : DbContext
    {
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }

}
