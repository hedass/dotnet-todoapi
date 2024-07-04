using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using onboarding.bll.Interfaces;

namespace onboarding.bll.Services
{
    public class TelemetryClientService : ITelemetryClientService
    {
        private readonly TelemetryClient _telemetryClient;

        public TelemetryClientService(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackDependency(DependencyTelemetry telemetry)
        {
            _telemetryClient.TrackDependency(telemetry);
        }

        public void TrackException(Exception exception)
        {
            _telemetryClient.TrackException(exception);
        }

        public IOperationHolder<DependencyTelemetry> StartOperation(DependencyTelemetry telemetry)
        {
            return _telemetryClient.StartOperation(telemetry);
        }

        public void StopOperation(IOperationHolder<DependencyTelemetry> operation)
        {
            _telemetryClient.StopOperation(operation);
        }
    }
}
