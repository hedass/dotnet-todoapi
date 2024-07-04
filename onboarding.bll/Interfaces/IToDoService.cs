using onboarding.bll.Services;
using onboarding.dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace onboarding.bll.Interfaces
{
    public interface IToDoService
    {
        public List<ToDoItem> GetAllToDos();

        public ToDoItem AddToDo(string title);

        public ToDoItem? GetToDoById(int id);

        public bool DeleteToDoById(int id);

        public ToDoItem? UpdateToDo(int id, string title, bool completed);

        public List<ToDoItem> GetToDoByTitle(string query);
    }
}
