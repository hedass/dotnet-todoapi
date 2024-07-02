using onboarding.dal;

namespace onboarding.bll
{
    public class ToDoService
    {
        private readonly UnitOfWork _unitOfWork;

        public ToDoService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<ToDoItem> GetAllToDos() { return _unitOfWork.ToDoRepository.GetAllToDos(); }

        public ToDoItem AddToDo(string title)
        {
            string trimmedTitle = title.ToLower().Trim();
            ToDoItem toDo = _unitOfWork.ToDoRepository.AddToDo(trimmedTitle);
            _unitOfWork.Save();
            return toDo;
        }

        public ToDoItem? GetToDoById(int id)
        { 
            return _unitOfWork.ToDoRepository.GetToDoById(id);
        }

        public bool DeleteToDoById(int id)
        {
            var toDo = _unitOfWork.ToDoRepository.GetToDoById(id);
            if (toDo != null)
            {
                _unitOfWork.ToDoRepository.DeleteToDo(toDo);
                _unitOfWork.Save();
                return true;
            }
            return false;
        }

        public ToDoItem? UpdateToDo(int id, string title, bool completed)
        {
            var toDo = _unitOfWork.ToDoRepository.GetToDoById(id);
            if (toDo != null)
            {
                toDo.Title = title;
                toDo.Completed = completed;

                _unitOfWork.ToDoRepository.UpdateToDo(toDo);
                _unitOfWork.Save();

                return toDo;
            }
            return null;
        }

        public List<ToDoItem> GetToDoByTitle(string query) {
            string normalizeQuery = query.ToLower();
            return _unitOfWork.ToDoRepository.GetToDoByTitle(normalizeQuery);
        }
    }
}
