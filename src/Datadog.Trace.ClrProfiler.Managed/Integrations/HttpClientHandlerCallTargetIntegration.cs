using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Datadog.Trace.ClrProfiler.CallTarget;
using Datadog.Trace.ClrProfiler.DuckTyping;
using Datadog.Trace.ClrProfiler.Helpers;
using Datadog.Trace.Logging;

#pragma warning disable SA1201 // Elements must appear in the correct order
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented

namespace Datadog.Trace.ClrProfiler.Integrations
{
    [InterceptMethod(
            TargetAssembly = "System.Net.Http",
            TargetType = "System.Net.Http.HttpClientHandler",
            TargetMethod = "SendAsync",
            TargetSignatureTypes = new[] { ClrNames.HttpResponseMessageTask, ClrNames.HttpRequestMessage, ClrNames.CancellationToken },
            TargetMinimumVersion = "4",
            TargetMaximumVersion = "4",
            MethodReplacementAction = MethodReplacementActionType.CallTargetModification)]
    public static class HttpClientHandlerCallTargetIntegration
    {
        private const string IntegrationName = "HttpMessageHandler";
        private static readonly Vendors.Serilog.ILogger Log = DatadogLogging.GetLogger(typeof(HttpClientHandlerCallTargetIntegration));

        public static CallTargetState OnMethodBegin<TInstance, TArg1>(TInstance instance, TArg1 requestMessage, CancellationToken cancellationToken)
            where TInstance : IHttpClientHandler, IDuckType
            where TArg1 : IHttpRequestMessage
        {
            Scope scope = null;

            if (IsTracingEnabled(requestMessage.Headers))
            {
                scope = ScopeFactory.CreateOutboundHttpScope(Tracer.Instance, requestMessage.Method.Method, requestMessage.RequestUri, IntegrationName);
                if (scope != null)
                {
                    scope.Span.SetTag("http-client-handler-type", instance.Type.ToString());
                    scope.Span.SetTag("prototype-version", "v11");
                    scope.Span.SetTag("use-proxy", instance.UseProxy.ToString());
                    scope.Span.SetTag("max-connections-per-server", instance.MaxConnectionsPerServer.ToString());
                    scope.Span.SetTag("ducktype-proxy-name", instance.ToString());

                    // add distributed tracing headers to the HTTP request
                    SpanContextPropagator.Instance.Inject(scope.Span.Context, new ReflectionHttpHeadersCollection(((IDuckType)requestMessage.Headers).Instance));
                }
            }

            return new CallTargetState(scope);
        }

        public static Task OnMethodEnd(Task responseMessage, Exception exception, CallTargetState state)
        {
            Scope scope = (Scope)state.State;
            if (scope != null)
            {
                scope.Span.SetTag("Control flow return", "true");
            }

            return responseMessage;
        }

        public static IHttpResponseMessage OnAsyncMethodEnd(IHttpResponseMessage responseMessage, Exception exception, CallTargetState state)
        {
            Scope scope = (Scope)state.State;
            try
            {
                if (scope != null)
                {
                    scope.Span.SetTag("integration", "CallTarget");
                    scope.Span.SetTag("integration-version", "v11");
                    if (exception is null)
                    {
                        scope.Span.SetTag(Tags.HttpStatusCode, responseMessage.StatusCode.ToString());
                    }
                    else
                    {
                        scope.Span.SetException(exception);
                    }
                }
            }
            finally
            {
                scope?.Dispose();
            }

            return responseMessage;
        }

        private static bool IsTracingEnabled(IRequestHeaders headers)
        {
            if (headers.Contains(HttpHeaderNames.TracingEnabled))
            {
                var headerValues = headers.GetValues(HttpHeaderNames.TracingEnabled);
                if (headerValues != null && headerValues.Any(s => string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)))
                {
                    // tracing is disabled for this request via http header
                    return false;
                }
            }

            return true;
        }

        public interface IHttpRequestMessage
        {
            IHttpMethod Method { get; }

            Uri RequestUri { get; }

            IVersion Version { get; }

            IRequestHeaders Headers { get; }
        }

        public interface IHttpMethod
        {
            string Method { get; }
        }

        public interface IVersion
        {
            int Major { get; }

            int Minor { get; }

            int Build { get; }
        }

        public interface IRequestHeaders
        {
            bool Contains(string name);

            IEnumerable<string> GetValues(string name);
        }

        public interface IHttpResponseMessage
        {
            int StatusCode { get; }
        }

        public interface IHttpClientHandler
        {
            bool UseProxy { get; set; }

            int MaxConnectionsPerServer { get; set; }
        }
    }
}
