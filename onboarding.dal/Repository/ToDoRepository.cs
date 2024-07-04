using Microsoft.EntityFrameworkCore;
using onboarding.dal.Interfaces;
using onboarding.dal.Models;

namespace onboarding.dal.Repository
{

    public class ToDoRepository : IToDoRepository
    {
        private readonly DbSet<ToDoItem> dbSet;

        public ToDoItemDbContext Context { get; private set; }

        public ToDoRepository(ToDoItemDbContext context)
        {
            Context = context;
            dbSet = context.Set<ToDoItem>();
        }

        public ToDoItem AddToDo(string title)
        {
            var toDo = new ToDoItem { Title = title };
            dbSet.Add(toDo);
            return toDo;
        }

        public ToDoItem? GetToDoById(int id)
        {
            return dbSet.Find(id);
        }

        public List<ToDoItem> GetAllToDos()
        {
            return dbSet.ToList();
        }

        public void UpdateToDo(ToDoItem toDo)
        {
            dbSet.Attach(toDo);
            Context.Entry(toDo).State = EntityState.Modified;
        }

        public void DeleteToDo(ToDoItem toDo)
        {
            if (toDo == null) return;

            dbSet.Remove(toDo);
        }

        public List<ToDoItem> GetToDoByTitle(string title)
        {
            List<ToDoItem> filteredToDos = dbSet.Where(t => t.Title.ToLower().Contains(title)).OrderBy(t => t.Title).ToList();
            return filteredToDos;
        }
    }
}
