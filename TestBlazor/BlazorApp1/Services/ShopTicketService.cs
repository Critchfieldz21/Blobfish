using System.Collections;

namespace BackendLibrary
{
    public class ShopTicketService
    {
    
        public int Size { get; set; } = 0;
        public List<ShopTicket?> CurrentTicket = new();


        public List<ShopTicket?> History = new();

    public int CurrentIndex { get; private set; } = 0;

    public event Action<int>? HistoryIndexChanged;

        public void AddTicket(ShopTicket ticket)
        {
            CurrentTicket.Add(ticket);
            History.Add(ticket);  
            Size++;
        }

        public ShopTicket GetTicket(int index)
        {
            return CurrentTicket[index - 1];
        }

        public ShopTicket GetHistoryTicket(int index)
        {
            return History[index];
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
            return History[CurrentIndex];
        }

        public void ClearCurrentTickets()
        {
            CurrentTicket.Clear();
            Size = 0;
        }
    }
}
