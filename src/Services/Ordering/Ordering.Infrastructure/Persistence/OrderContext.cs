using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContext:DbContext
    {


        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }
        //private readonly DbContextOptions<OrderContext> _options;
        //public OrderContext(DbContextOptions<OrderContext> options):base(options)
        //{
        //    _options = options ?? throw new ArgumentNullException(nameof(options));
        //}

        public DbSet<Order> Orders { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.CreatedBy = "System";
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedDate = DateTime.Now;
                        entry.Entity.LastModifiedBy = "System";
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
