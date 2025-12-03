using ChatGPTClone.Models;
using ChatGPTClone.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatGPTClone.Controllers
{
    public class HomeController : Controller
    {
        private readonly OpenAIService _chatService;
        private static List<Message> _history = new();

        public HomeController(OpenAIService chatService)
        {
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            var vm = new ChatViewModel { Messages = _history };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Send(ChatViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.UserInput))
            {
                // Add user message
                _history.Add(new Message { Role = "user", Content = vm.UserInput });

                // Send to OpenAI
                var aiResponse = await _chatService.SendMessageAsync(_history);

                // Add response
                _history.Add(new Message { Role = "assistant", Content = aiResponse });
            }

            return RedirectToAction("Index");
        }
    }
}
