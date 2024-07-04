using Microsoft.Extensions.Caching.Distributed;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using onboarding.dal;
using onboarding.bll.Interfaces;
using onboarding.dal.Models;
using Microsoft.ApplicationInsights;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Microsoft.ApplicationInsights.DataContracts;

namespace onboarding.bll.Services
{
    public class ToDoService: IToDoService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly TelemetryClient _telemetryClient;
        
        private readonly UnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        public ToDoService(UnitOfWork unitOfWork, IRedisService redisService, TelemetryClient telemetryClient)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisService;
            _telemetryClient = telemetryClient;

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
            string queueName = "notificationQueue";
            var dependencyTelemetry = new DependencyTelemetry
            {
                Type = "RabbitMQ",
                Target = queueName,
                Data = msg,
                Name = "Send Message"
            };

            var operation = _telemetryClient.StartOperation(dependencyTelemetry);
            try
            {

                var body = Encoding.UTF8.GetBytes(msg);
                _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

                _telemetryClient.TrackDependency(dependencyTelemetry);
                operation.Telemetry.Success = true;
            }
            catch (Exception ex)
            {
                operation.Telemetry.Success = false;

                _telemetryClient.TrackException(ex); 
                _telemetryClient.TrackDependency(dependencyTelemetry);
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }

        public List<ToDoItem> GetAllToDos() { return _unitOfWork.ToDoRepository.GetAllToDos(); }

        public ToDoItem AddToDo(string title)
        {
            string trimmedTitle = title.ToLower().Trim();
            ToDoItem toDo = _unitOfWork.ToDoRepository.AddToDo(trimmedTitle);
            _unitOfWork.Save();

            _redisService.SetStringAsync($"todoitem:{toDo.Id}", toDo);

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
