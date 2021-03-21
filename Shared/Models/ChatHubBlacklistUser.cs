using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubBlacklistUser : ChatHubBaseModel
    {

        public int ChatHubUserId { get; set; }
        public string BlacklistUserDisplayName { get; set; }

        [NotMapped]
        public virtual ChatHubUser User { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubBlacklistUser> BlacklistUserRooms { get; set; }

    }
}
