using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using TicketSys.Domain;
using System.Reflection;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace TicketSys.Persistence
{
    #region Not Cool
    // http://stackoverflow.com/a/19130718
    internal static class SuperUglyHack
    {
        private static void FixEfProviderBadBadBad()
        {
            // The compiler mysteriously decides to simply exclude EntityFramework.SqlServer.dll
            // unless I do this:
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
            // Compliments of Redmond, WA.
        }
    }
    #endregion

    public class Store<T> : IStore<T>
    {
        private DbContext _context;
        private DbSet _dbSet;

        public Store(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set(typeof(T));
        }

        private class ScopeChanges : IDisposable
        {
            public ScopeChanges(DbContext context)
            {
                _context = context;
            }

            public void Dispose()
            {
                _context.SaveChanges();
            }

            private DbContext _context;
        }

        #region IStore<>
        public void Add(T item)
        {
             _dbSet.Add(item);
        }

        public void Remove(T item)
        {
            _dbSet.Remove(item);
        }

        public void Update(T item)
        {
            _dbSet.Attach(item);
        }

        public void Attach(T item)
        {
            _dbSet.Attach(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dbSet.AsQueryable().Cast<T>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dbSet.AsQueryable().GetEnumerator();
        }

        public Type ElementType
        {
            get { return _dbSet.AsQueryable().ElementType;  }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { return _dbSet.AsQueryable().Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _dbSet.AsQueryable().Provider; }
        }

        public IDisposable ScopedChanges()
        {
            return new ScopeChanges(_context);
        }
        #endregion
    }

    public static class RepositoryFactory
    {
        public static IRepository MakeTicketingRepository()
        {
            return new Repository(new TicketingContext());
        }
    }

    public class Repository : IRepository
    {
        private DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        public IStore<T> GetStoreOf<T>()
        {
            return new Store<T>(_context);
        }

        public IEnumerable<Type> GetAvailableTypes()
        {
            // Get all of the public, instance properties
            return typeof(TicketingContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)

                // Filter by...
                .Where(x =>
                    {
                        // Restricting to generically typed properties
                        return x.PropertyType.IsGenericType &&
                            (
                                // Checking if the property is an IDbSet<> itself, or...
                                (x.PropertyType.IsInterface && x.PropertyType.GetGenericTypeDefinition() == typeof(IDbSet<>)) ||
                                // Checking if the property implements IDbSet<>
                                (x.PropertyType.GetInterfaces().Where(m => m.IsGenericType).Select(m => m.GetGenericTypeDefinition()).Any(m => m == typeof(IDbSet<>)))
                            );
                    })

                // Then dump all of the resulting type arguments into one Enumerable
                .SelectMany(x => x.PropertyType.GetGenericArguments());
        }

        public void Dispose()
        {
            if (_context != null)
                _context.Dispose();
        }
    }

    public class TicketSysDbConfiguration : DbConfiguration
    {
        public TicketSysDbConfiguration()
        {
            SetDefaultConnectionFactory(new SqlConnectionFactory());
        }
    }

    public class TicketingContext : DbContext
    {
        public IDbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Make all table names singular by default.
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Make all string columns required by default.
            modelBuilder.Properties<string>()
                .Configure(x => x.IsRequired());

            base.OnModelCreating(modelBuilder);
        }
    }
}
