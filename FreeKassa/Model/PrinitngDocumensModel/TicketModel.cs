using System;

namespace FreeKassa.Model.PrinitngDocumensModel
{
    public class TicketModel
    {
        public byte[] Logo { get; set; }
        public string Name { get; set; }
        public string Places { get; set; }
        public string DateTime { get; set; }
        public string Address { get; set; }
    }
}