using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoomChatHubWhitelistUser : ChatHubBaseModel
    {
        public int ChatHubRoomId { get; set; }
        public int ChatHubWhitelistUserId { get; set; }


        [NotMapped]
        public virtual ChatHubRoom Room { get; set; }
        [NotMapped]
        public virtual ChatHubWhitelistUser WhitelistUser { get; set; }
    }
}
