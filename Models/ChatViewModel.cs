using System.Collections.Generic;

namespace ChatGPTClone.Models
{
    public class ChatViewModel
    {
        public List<Message> Messages { get; set; } = new List<Message>();
        public string UserInput { get; set; }
    }
}
