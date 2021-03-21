using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;
using Oqtane.Shared.Enums;

namespace Oqtane.Shared.Models
{

    public class ChatHubUser : User
    {

        [NotMapped]
        public ChatHubRoomLevelType RoomLevel { get; set; }

        [NotMapped]
        public bool UserlistItemCollapsed { get; set; }

        [NotMapped]
        public virtual IList<ChatHubRoomChatHubUser> UserRooms { get; set; }

        [NotMapped]
        public virtual IList<ChatHubConnection> Connections { get; set; }

        [NotMapped]
        public virtual ChatHubSettings Settings { get; set; }

        [NotMapped]
        public virtual ChatHubCam Cam { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubIgnore> Ignores { get; set; }

    }
}