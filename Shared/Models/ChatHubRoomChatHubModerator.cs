using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoomChatHubModerator : ChatHubBaseModel
    {

        public int ChatHubRoomId { get; set; }
        public int ChatHubModeratorId { get; set; }


        [NotMapped]
        public virtual ChatHubRoom Room { get; set; }
        [NotMapped]
        public virtual ChatHubModerator Moderator { get; set; }

    }
}
