using Grpc.Core;

namespace GrpcService.Client.GrpcCore.Models
{
    public class GrpcResponseModel<T> where T : class
    {
        /// <summary>
        /// Response payload
        /// </summary>
        public T? Payload { get; set; }

        /// <summary>
        /// Metadata headers
        /// </summary>
        public Metadata? Headers { get; set; }

        /// <summary>
        /// Grpc response status
        /// </summary>
        public Status? Status { get; set; }

        /// <summary>
        /// If you want to skip saving of the metadata headers
        /// </summary>
        public bool IsHeadersSkipped {  get; set; } 
    }
}
