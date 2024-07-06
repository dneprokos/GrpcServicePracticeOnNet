using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcService.Client.GrpcCore.Utils
{
    public class GrpcChannelHelper
    {
        public static Channel NewInsecuredGrpcCoreChannel(string target)
            => new(target, ChannelCredentials.Insecure);

        public static GrpcChannel NewInsecuredGrpcNetChannel(string target)
            => GrpcChannel.ForAddress(target);
    }
}
