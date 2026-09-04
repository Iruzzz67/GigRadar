using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GigRadarApi.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string TicketType { get; set; } = "Regular";
        public decimal Price { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerPhone { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public DateTime? BuyerDateOfBirth { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
