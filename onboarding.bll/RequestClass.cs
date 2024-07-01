
using System.ComponentModel.DataAnnotations;

namespace onboarding.bll
{
    public class ToDoItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;
    }
}
