using Microsoft.EntityFrameworkCore;
using onboarding.dal.Models;

namespace onboarding.dal
{
    public class ToDoItemDbContext : DbContext
    {
        public ToDoItemDbContext(DbContextOptions<ToDoItemDbContext> options) : base(options) { }

        public DbSet<ToDoItem> ToDoItems { get; set; }
    }
}
