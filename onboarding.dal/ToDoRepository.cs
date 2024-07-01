namespace onboarding.dal
{

    public class ToDoRepository
    {
        private static List<ToDoItem> toDos = new List<ToDoItem>();

        public ToDoItem AddToDo(string title)
        {
            int toDoId = toDos.Count + 1;
            var toDo = new ToDoItem { Id = toDoId, Title = title };
            toDos.Add(toDo);
            return toDo;
        }

        public ToDoItem? GetToDoById(int id)
        {
            return toDos.Find(toDo => toDo.Id == id);
        }

        public List<ToDoItem> GetAllToDos()
        {
            return toDos;
        }

        public ToDoItem? UpdateToDo(int todoId, string title, bool completed)
        {
            var todo = GetToDoById(todoId);
            if (todo != null)
            {
                todo.Title = title;
                todo.Completed = completed;
                return todo;
            }
            return null;
        }

        public bool DeleteToDoById(int todoId)
        {
            var todo = GetToDoById(todoId);
            if (todo != null)
            {
                toDos.Remove(todo);
                return true;
            }
            return false;
        }
    }
}
