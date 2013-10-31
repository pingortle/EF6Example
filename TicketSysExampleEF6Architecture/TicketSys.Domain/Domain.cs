using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSys.Domain
{
    #region Interfaces
    public interface IRepository : IDisposable
    {
        IStore<T> GetStoreOf<T>();
        IEnumerable<Type> GetAvailableTypes();
    }

    public interface ISee<out T> : IQueryable<T>, IEnumerable<T> { }

    public interface ITake<in T>
    {
        void Add(T item);
        void Remove(T item);
        void Update(T item);
        void Attach(T item);
    }

    public interface IStore<in T1, out T2> : ITake<T1>, ISee<T2>
    {
        IDisposable ScopedChanges();
    }

    public interface IStore<T> : IStore<T, T> { }
    #endregion

    #region Domain Classes
    public class Customer
    {
        public int Id { get; set; }

        public string BusinessName { get; set; }
        public string ContactName { get; set; }
        public string Phone { get; set; }

        public virtual List<Ticket> Tickets { get; set; }
    }

    public class Ticket
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public int CustomerId { get; set; }
    }
    #endregion
}
