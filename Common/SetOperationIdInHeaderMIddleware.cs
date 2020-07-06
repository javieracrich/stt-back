using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using static Common.Constants;

namespace Common
{
    public class SetOperationIdInHeaderMiddleware
    {
        private readonly RequestDelegate next;

        public SetOperationIdInHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                if (headers.ContainsKey(TelemetryProperties.OperationId))
                {
                    headers.Remove(TelemetryProperties.OperationId);
                }

                if (context.Items.ContainsKey(TelemetryProperties.OperationId))
                {
                    var operationId = (string)context.Items[TelemetryProperties.OperationId];
                    headers.Add(TelemetryProperties.OperationId, operationId);
                }

                return Task.FromResult(0);
            });

            return this.next(context);
        }
    }
}
