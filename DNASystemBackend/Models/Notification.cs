using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DNASystemBackend.Models
{
    public class Notification
    {
        public string NotificationId { get; set; } 

        public string UserId { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public string NotificationType { get; set; }

        public bool IsRead { get; set; }


        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}