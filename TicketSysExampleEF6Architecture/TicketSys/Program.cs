using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketSys.Domain;
using TicketSys.Persistence;
using Ninject;
using Ninject.Extensions.NamedScope;

namespace TicketSys
{
    class DoStuff
    {
        IRepository repo;

        public DoStuff(IRepository repo)
        {
            this.repo = repo;
        }

        public void Now()
        {
                        int count = 0;
            foreach (Type t in repo.GetAvailableTypes())
            {
                Console.WriteLine(" {0}\t{1}", (++count).ToString(), t.ToString());
            }

            IStore<Customer> customers = repo.GetStoreOf<Customer>();
            using (customers.ScopedChanges())
            {
                customers.Add(new Customer { BusinessName = "Test", ContactName = "Test", Phone = "+X (XXX) XXX XXXX" });
                customers.Add(new Customer { BusinessName = "Test2", ContactName = "Test", Phone = "+X (XXX) XXX XXXX" });
                customers.Add(new Customer { BusinessName = "Test3", ContactName = "Test", Phone = "+X (XXX) XXX XXXX" });
                customers.Add(new Customer { BusinessName = "Test", ContactName = "Test", Phone = "+X (XXX) XXX XXXX" });
            }

            Console.WriteLine("");

            foreach (Customer c in repo.GetStoreOf<Customer>())
            {
                Console.WriteLine(c.BusinessName);
            }

            Console.Write("Press any key to continue.");
            Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("Rolling back changes...");

            using (customers.ScopedChanges())
            {
                foreach (var c in customers
                    .AsEnumerable()
                    .Where(x =>
                        x.ContactName.ToLower().Contains("test") ||
                        x.BusinessName.ToLower().Contains("test") ||
                        !x.Phone.Any(m => char.IsLetter(m))))
                {
                    customers.Remove(c);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();

            kernel.Bind<IRepository>().To<TicketingRepository>().InParentScope();
            kernel.Get<DoStuff>().Now();

            Console.WriteLine("Exiting...");
        }
    }
}
