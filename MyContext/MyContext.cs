using DbContextTests.Model;
using MyContext.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DbContextTests
{
    public class MyContext : DbContext
    {
        public bool IsDispsed { get; private set; }
        private static volatile int instanceCount = 0;
        private static volatile int totalInstancesCreated = 0;
        public static int InstanceCount => instanceCount;
        public static int TotalInstancesCreated => totalInstancesCreated;

        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<UserPreferences> UserPreferences { get; set; }

        public MyContext()
        {
            Interlocked.Increment(ref instanceCount);
            Interlocked.Increment(ref totalInstancesCreated);
        }

        public static void ResetCounters()
        {
            instanceCount = 0;
            totalInstancesCreated = 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDispsed = true;
            Interlocked.Decrement(ref instanceCount);
        }
    }
}
