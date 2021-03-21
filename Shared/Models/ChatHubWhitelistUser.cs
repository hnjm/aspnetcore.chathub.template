using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubWhitelistUser : ChatHubBaseModel
    {

        public int ChatHubUserId { get; set; }
        public string WhitelistUserDisplayName { get; set; }

        [NotMapped]
        public virtual ChatHubUser User { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubWhitelistUser> WhitelistUserRooms { get; set; }

    }
}
