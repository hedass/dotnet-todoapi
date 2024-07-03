using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using onboarding.dal;
using onboarding.bll.Interfaces;
using onboarding.dal.Models;

namespace onboarding.bll.Services
{
    public class ToDoService: IToDoService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly UnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        public ToDoService(UnitOfWork unitOfWork, IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisService;

            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "notificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void SendNotification(string msg)
        {
            var body = Encoding.UTF8.GetBytes(msg);
            _channel.BasicPublish(exchange: "", routingKey: "notificationQueue", basicProperties: null, body: body);
        }

        public List<ToDoItem> GetAllToDos() { return _unitOfWork.ToDoRepository.GetAllToDos(); }

        public ToDoItem AddToDo(string title)
        {
            string trimmedTitle = title.ToLower().Trim();
            ToDoItem toDo = _unitOfWork.ToDoRepository.AddToDo(trimmedTitle);
            _unitOfWork.Save();

            _redisService.SetStringAsync<ToDoItem>($"todoitem:{toDo.Id}", toDo);

            SendNotification($"ToDo item added: {toDo.Title}");

            return toDo;
        }

        public ToDoItem? GetToDoById(int id)
        {
            var toDo = _redisService.GetString<ToDoItem>($"todoitem:{id}");
            toDo ??= _unitOfWork.ToDoRepository.GetToDoById(id);

            return toDo;
        }

        public bool DeleteToDoById(int id)
        {
            var toDo = _unitOfWork.ToDoRepository.GetToDoById(id);
            if (toDo != null)
            {
                _unitOfWork.ToDoRepository.DeleteToDo(toDo);
                _unitOfWork.Save();

                _redisService.RemoveAsync($"todoitem:{toDo.Id}");

                SendNotification($"ToDo item deleted: {toDo.Title}");

                return true;
            }
            return false;
        }

        public ToDoItem? UpdateToDo(int id, string title, bool completed)
        {
            var toDo = _unitOfWork.ToDoRepository.GetToDoById(id);
            if (toDo != null)
            {
                string oldTitle = toDo.Title;
                bool oldCompleted = toDo.Completed;

                toDo.Title = title;
                toDo.Completed = completed;

                _unitOfWork.ToDoRepository.UpdateToDo(toDo);
                _unitOfWork.Save();

                _redisService.SetStringAsync($"todoitem:{toDo.Id}", toDo);

                SendNotification($"ToDo item updated: \n{(toDo.Title != oldTitle ? $"{oldTitle} -> {title}\n" : "")}{(toDo.Completed != oldCompleted ? $"{oldCompleted} -> {completed}\n" : "")}");

                return toDo;
            }
            return null;
        }

        public List<ToDoItem> GetToDoByTitle(string query)
        {
            string normalizeQuery = query.ToLower();
            return _unitOfWork.ToDoRepository.GetToDoByTitle(normalizeQuery);
        }
    }
}
