using System.Collections.Generic;

namespace TicketSys.Domain
{
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
