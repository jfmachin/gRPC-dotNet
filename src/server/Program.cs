using Blog;
using Grpc.Core;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using server.Services;
using System.IO;

namespace server {
    class Program {
        const int port = 50051;
        static void Main(string[] args) {
            Server server = null;
            try {
                var reflectionService = new ReflectionServiceImpl(BlogService.Descriptor, ServerReflection.Descriptor);
                server = new Server() {
                    Services = { 
                        BlogService.BindService(new BlogServiceImpl()),
                        ServerReflection.BindService(reflectionService)
                    },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };

                server.Start();
                System.Console.WriteLine($"The server is listening on port: {port}");
                System.Console.ReadKey();
            }
            catch (IOException e) {
                System.Console.WriteLine($"The server has failed to start: {e.Message}");
                throw;
            }
            finally {
                if (server != null)
                    server.ShutdownAsync().Wait();
            }
        }
    }
}