using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{

    public class ChatHubUser
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public string DisplayName { get; set; }

        [NotMapped]
        public bool UserlistItemCollapsed { get; set; }

        [NotMapped]
        public virtual IList<ChatHubRoomChatHubUser> UserRooms { get; set; }

        [NotMapped]
        public virtual IList<ChatHubConnection> Connections { get; set; }

        [NotMapped]
        public virtual ChatHubSetting Settings { get; set; }

        [NotMapped]
        public virtual ChatHubCam Cam { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubIgnore> Ignores { get; set; }

    }
}