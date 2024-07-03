
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace onboarding.bll.DTO
{
    public class ToDoItemDTO
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        [DefaultValue(false)]
        public bool Completed { get; set; }
    }
}
