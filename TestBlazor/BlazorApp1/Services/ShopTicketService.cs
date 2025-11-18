using System.Collections;
using System.Data;

namespace BackendLibrary
{
    public class ShopTicketService
    {
    
        public int Size { get; set; } = 0;
        public List<ShopTicket?> CurrentTicket { get; set; } = new();


        public List<ShopTicket?> History { get; set; } = new();
        //boolean value to see if duplicate ticket is being added to curr
        private bool dupe = false;

    public int CurrentIndex { get; private set; } = 0;

    public event Action<int>? HistoryIndexChanged;

        public void AddTicket(ShopTicket ticket, bool isDuplicate)
        {
            dupe = isDuplicate;
            CurrentTicket.Add(ticket);
            Size++;
        }
        

        public ShopTicket GetTicket(int index)
        {
            if(Size == 0 || index > Size)
            {
                return null;
            }
            else
            {
                return CurrentTicket[index - 1];
            }
        }

        public ShopTicket GetHistoryTicket(int index)
        {
            return History[index - 1];
        }

        public void SetCurrentTicket(List<ShopTicket?> tickets)
        {
            Size = tickets.Count;
            CurrentTicket = tickets;
        }

        public int HistorySize()
        {
            return History.Count;
        }

        public void SetCurrentHistoryIndex(int index)
        {
            CurrentIndex = index;
            try
            {
                HistoryIndexChanged?.Invoke(index);
            }
            catch
            {
                // Ignore exceptions from event handlers
            }
        }

        public ShopTicket GetCurrentHistoryTicket()
        {
            return History[CurrentIndex - 1];
        }

        public void ClearCurrentTickets()
        {
            if(dupe)
            {
                CurrentTicket.Clear();
                dupe = false;
                Size = 0;
            }
            else
            {
                History.AddRange(CurrentTicket.Where(ticket => ticket != null)!);
                CurrentTicket.Clear();
                Size = 0;
            }
        }
    }
}
