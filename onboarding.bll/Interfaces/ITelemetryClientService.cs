using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace onboarding.bll.Interfaces
{
    public interface ITelemetryClientService
    {
        void TrackDependency(DependencyTelemetry telemetry);
        void TrackException(Exception exception);
        IOperationHolder<DependencyTelemetry> StartOperation(DependencyTelemetry telemetry);
        void StopOperation(IOperationHolder<DependencyTelemetry> operation);
    }
}
