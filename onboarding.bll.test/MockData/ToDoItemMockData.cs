using onboarding.dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace onboarding.bll.test.MockData
{
    public static class ToDoItemMockData
    {
        private static readonly List<ToDoItem> InitialToDoItems = new()
        { 
                new() { Id = 1, Title = "Task 1" },
                new() { Id = 2, Title = "Task 2" },
                new() { Id = 3, Title = "Task 3" },
                new() { Id = 4, Title = "Task 4" },
                new() { Id = 5, Title = "Task 5" },
                new() { Id = 6, Title = "Task 6" },
                new() { Id = 7, Title = "Task 7" },
                new() { Id = 8, Title = "Task 8" },
        };
        public static List<ToDoItem> ToDoItems { get; set; } = CloneInitialData();

        public static List<ToDoItem> CloneInitialData()
        {
            return InitialToDoItems.Select(item => new ToDoItem
            {
                Id = item.Id,
                Title = item.Title,
                Completed = item.Completed
            }).ToList();
        }
    }
}
