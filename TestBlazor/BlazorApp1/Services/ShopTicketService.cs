using System.Collections;
using System.Data;

namespace BackendLibrary
{
    /// <summary>
    /// This class is used to link Shop Tickets to the front end.
    /// </summary>
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

        public void ClearCurrentTickets()
        {
            // Move CurrentTicket items into History without creating duplicates.
            // If a ticket with the same piece mark exists, replace it; otherwise add.
            if (CurrentTicket != null && CurrentTicket.Count > 0)
            {
                foreach (var t in CurrentTicket)
                {
                    if (t == null) continue;

                    int existingIdx = -1;
                    // Prefer matching by FileNamePieceMark when available; fallback to FileName
                    if (!string.IsNullOrWhiteSpace(t.FileNamePieceMark))
                    {
                        existingIdx = History.FindIndex(h => h != null && h.FileNamePieceMark == t.FileNamePieceMark);
                    }
                    if (existingIdx < 0 && !string.IsNullOrWhiteSpace(t.FileName))
                    {
                        existingIdx = History.FindIndex(h => h != null && h.FileName == t.FileName);
                    }

                    if (existingIdx >= 0)
                    {
                        History[existingIdx] = t;
                    }
                    else
                    {
                        History.Add(t);
                    }
                }
            }

            CurrentTicket.Clear();
            dupe = false;
            Size = 0;
        }
    }
}
