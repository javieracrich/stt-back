using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using static Common.Constants;

namespace Common
{
    public class SttTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public SttTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            ISupportProperties propTelemetry = (ISupportProperties)telemetry;

            var operationId = telemetry.Context.Operation.Id;

            var context = this.httpContextAccessor.HttpContext;

            if (context == null)
            {
                return;
            }

            if (operationId != null && !context.Items.ContainsKey(TelemetryProperties.OperationId))
            {
                context.Items.Add(TelemetryProperties.OperationId, operationId);
            }

            if (telemetry is RequestTelemetry)
            {
                if (!propTelemetry.Properties.ContainsKey(TelemetryProperties.IpAddress) &&
                    context.Connection?.RemoteIpAddress != null)
                {
                    propTelemetry.Properties.Add(TelemetryProperties.IpAddress, context.Connection.RemoteIpAddress.ToString());
                }

                if (!propTelemetry.Properties.ContainsKey(TelemetryProperties.ClientApp) && 
                    context.Request.Headers.ContainsKey(TelemetryProperties.ClientApp))
                {
                    var clientApp = context.Request.Headers[TelemetryProperties.ClientApp][0];
                    propTelemetry.Properties.Add(TelemetryProperties.ClientApp, clientApp);
                }
            }
        }
    }
}
