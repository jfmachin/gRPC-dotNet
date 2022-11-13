using Blog;
using Grpc.Core;
using server.Services;
using System.IO;

namespace server {
    class Program {
        const int port = 50051;
        static void Main(string[] args) {
            Server server = null;
            try {
                server = new Server() {
                    Services = { BlogService.BindService(new BlogServiceImpl()) },
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