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
            string trimmedTitle = title.ToLower().Trim();
            return _repository.AddToDo(trimmedTitle);
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

        public List<ToDoItem> GetToDoByTitle(string query) {
            string normalizeQuery = query.ToLower();
            return _repository.GetToDoByTitle(normalizeQuery);
        }
    }
}
