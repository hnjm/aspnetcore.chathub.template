using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubRoom : ChatHubBaseModel
    {

        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string BackgroundColor { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string OneVsOneId { get; set; }
        public int CreatorId { get; set; }
        
        [NotMapped]
        public string MessageInput { get; set; }
        [NotMapped]
        public int UnreadMessages { get; set; } = 0;
        [NotMapped]
        public bool ShowUserlist { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubMessage> Messages { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubUser> RoomUsers { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubUser> Users { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubModerator> RoomModerators { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubModerator> Moderators { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubWhitelistUser> RoomWhitelistUsers { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubWhitelistUser> WhitelistUsers { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubRoomChatHubBlacklistUser> RoomBlacklistUsers { get; set; }
        [NotMapped]
        public virtual ICollection<ChatHubBlacklistUser> BlacklistUsers { get; set; }
        [NotMapped]
        public ICollection<ChatHubWaitingRoomItem> WaitingRoomItems { get; set; } = new List<ChatHubWaitingRoomItem>();
        [NotMapped]
        public virtual ChatHubUser Creator { get; set; }

    }
}
