using Microsoft.EntityFrameworkCore;
using onboarding.dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onboarding.dal.Interfaces
{
    public interface IToDoRepository
    {

        public ToDoItem AddToDo(string title);

        public ToDoItem? GetToDoById(int id);

        public List<ToDoItem> GetAllToDos();

        public void UpdateToDo(ToDoItem toDo);

        public void DeleteToDo(ToDoItem toDo);

        public List<ToDoItem> GetToDoByTitle(string title);
    }
}
