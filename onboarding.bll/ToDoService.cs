using onboarding.dal;

namespace onboarding.bll
{
    public class ToDoService
    {
        private readonly ToDoRepository _repository;

        public ToDoService(ToDoRepository repository)
        {
            _repository = repository;
        }

        public List<ToDoItem> GetAllToDos() { return _repository.GetAllToDos(); }

        public ToDoItem AddToDo(string title)
        {
            return _repository.AddToDo(title);
        }

        public ToDoItem? GetToDoById(int id)
        { 
            return _repository.GetToDoById(id);
        }

        public bool DeleteToDoById(int id)
        {
            return _repository.DeleteToDoById(id);
        }

        public ToDoItem? UpdateToDo(int id, string title, bool completed)
        {
            return _repository.UpdateToDo(id, title, completed);
        }
    }
}
