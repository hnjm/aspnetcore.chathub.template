using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Models
{
    public class ChatHubCam : ChatHubBaseModel
    {

        public string Status { get; set; }

        public int ChatHubUserId { get; set; }
        [NotMapped]
        public virtual ChatHubUser User { get; set; }

    }
}
