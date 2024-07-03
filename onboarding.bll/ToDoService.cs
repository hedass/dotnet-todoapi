using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using onboarding.dal;

namespace onboarding.bll
{
    public class ToDoService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly UnitOfWork _unitOfWork;
        private readonly IDistributedCache _cache;
        public ToDoService(UnitOfWork unitOfWork, IDistributedCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;

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

            string serializedToDo = JsonSerializer.Serialize(toDo); 
            _cache.SetStringAsync($"todoitem:{toDo.Id}", serializedToDo, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            this.SendNotification($"ToDo item added: {toDo.Title}");

            return toDo;
        }

        public ToDoItem? GetToDoById(int id)
        {
            string serializedToDo = _cache.GetString($"todoitem:{id}");
            ToDoItem? toDo = serializedToDo == null ? _unitOfWork.ToDoRepository.GetToDoById(id) : JsonSerializer.Deserialize<ToDoItem>(serializedToDo);

            return toDo;
        }

        public bool DeleteToDoById(int id)
        {
            var toDo = _unitOfWork.ToDoRepository.GetToDoById(id);
            if (toDo != null)
            {
                _unitOfWork.ToDoRepository.DeleteToDo(toDo);
                _unitOfWork.Save();

                _cache.RemoveAsync($"todoitem:{toDo.Id}");

                this.SendNotification($"ToDo item deleted: {toDo.Title}");

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

                string serializedToDo = JsonSerializer.Serialize(toDo);
                _cache.SetStringAsync($"todoitem:{toDo.Id}", serializedToDo, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

                this.SendNotification($"ToDo item updated: \n{(toDo.Title != oldTitle ? $"{oldTitle} -> {title}\n" : "")}{(toDo.Completed != oldCompleted ? $"{oldCompleted} -> {completed}\n" : "")}");

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
