using Microsoft.ApplicationInsights.DataContracts;
using onboarding.bll.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace onboarding.bll.Services
{
    public class NotificationService : INotificationService
    {

        private readonly ITelemetryClientService _telemetryClient;

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public NotificationService(ITelemetryClientService telemetryClient)
        {

            _telemetryClient = telemetryClient;

            var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqHost,
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
    }
}
