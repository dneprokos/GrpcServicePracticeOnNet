using Grpc.Core;
using GrpcService.Client.GrpcCore.Models;
using GrpcService.Client.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GrpcService.Client.GrpcCore.Wrappers
{
    public class GrpcRequestBase
    {
        protected readonly ILogger? _logger;

        private readonly JsonSerializerSettings serializerSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public GrpcRequestBase()
        {
        }

        public GrpcRequestBase(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<GrpcResponseModel<T>> ExecuteRequestAsync<T>(
            Func<Task<AsyncUnaryCall<T>>> callFunction, 
            object? args,
            Metadata? headers,
            string? requestName,
            bool skipResponseHeader = true
            ) where T : class
        {
            LogRequest(args, headers!, requestName!);

            try
            {
                var call =  await callFunction();

                var responseModel = new GrpcResponseModel<T>
                {
                    Payload = await call.ResponseAsync,
                    IsHeadersSkipped = skipResponseHeader,
                    Headers = skipResponseHeader ? null : await call.ResponseHeadersAsync,
                    Status = call.GetStatus()
                };

                LogResponse(responseModel, requestName!);

                return responseModel;
            }
            catch (RpcException ex)
            {
                var response = new GrpcResponseModel<T>
                {
                    Payload = null,
                    IsHeadersSkipped = skipResponseHeader,
                    Headers = null,
                    Status = ex.Status
                };

                LogError(ex.Status, requestName!);

                return response;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error accured while calling {requestName} request");
                throw;
            }
        }

        #region Log methods

        private void LogRequest<T>(T message, Metadata? headers, string requestName)
        {
            if (_logger != null)
            {
                Dictionary<string, object> payloadDict = message!.ConvertPropertiesToDictionary();

                var objectToLog = new
                {
                    requestName,
                    headers = TranformHeadersToObjects(headers!),
                    payload = payloadDict.Count > 0 ? payloadDict : null
                };

                string formattedReq = JsonConvert.SerializeObject(objectToLog, serializerSettings);
                _logger.LogInformation("------------- GRPC Request -------------");
                _logger.LogInformation($"{formattedReq}");
            }
        }

        private void LogResponse<T>(GrpcResponseModel<T> response, string requestName) where T : class
        {
            if (_logger != null)
            {
                Dictionary<string, object> payloadDict = response!.ConvertPropertiesToDictionary();

                var objectToLog = new
                {
                    requestName,
                    status = response.Status!.Value.StatusCode.ToString(),
                    is_header_skipped = response.IsHeadersSkipped,
                    headers = response.IsHeadersSkipped ? null : TranformHeadersToObjects(response.Headers!),
                    payload = payloadDict.Count > 0 ? payloadDict : null
                };

                string formattedRes = JsonConvert.SerializeObject(objectToLog, serializerSettings);
                _logger.LogInformation("------------- GRPC Response -------------");
                _logger.LogInformation($"{formattedRes}");
            }
        }

        private void LogError(Status status, string requestName)
        {
            if (_logger != null)
            {
                var objectToLog = new
                {
                    requestName,
                    status = status.StatusCode.ToString(),
                    message = status.Detail
                };

                string formattedRes = JsonConvert.SerializeObject(objectToLog, serializerSettings);
                _logger.LogInformation("------------- GRPC Response -------------");
                _logger.LogInformation($"{formattedRes}");
            }
        }

        private static IEnumerable<object> TranformHeadersToObjects(Metadata headers)
        {
            if (headers != null)
            {
                return headers.Select(header => new
                {
                    key = header.Key,
                    value = header.Key!.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase) ?
                        "*****************" : header.Value,
                });
            }
            else return null!;
        }

        #endregion
    }
}
