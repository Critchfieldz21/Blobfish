using System.Collections;

namespace BackendLibrary
{
    public class ShopTicketService
    {
        // Add a field named size to track the size of the shopticket array
        public int Size { get; set; } = 0;
        public List<ShopTicket?> CurrentTicket = new();

        public void AddTicket(ShopTicket ticket)
        {
            CurrentTicket.Add(ticket);
            Size++;
        }

        public ShopTicket GetTicket(int size)
        {
            return CurrentTicket[size - 1];
        }

        public void RemoveTicket(ShopTicket ticket)
        {
            if (CurrentTicket.Remove(ticket))
            {
                Size--;
            }
        }

        public void ClearTickets()
        {
            CurrentTicket.Clear();
            Size = 0;
        }
    }
}

