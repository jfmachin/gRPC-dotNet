using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using static Blog.BlogService;

namespace server.Services {
    public class BlogServiceImpl : BlogServiceBase {
        private static MongoClient client = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase database = client.GetDatabase("blog");
        private static IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("blogs");
        
        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context) {
            var blog = request.Blog;
            BsonDocument doc = new BsonDocument("author_id", blog.AuthorId)
                .Add("title", blog.Title)
                .Add("content", blog.Content);

            collection.InsertOne(doc);
            var id = doc.GetValue("_id").ToString();
            blog.Id = id;
            return Task.FromResult(new CreateBlogResponse() { Blog = blog });
        }

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context) {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(request.BlogId));
            var doc = collection.Find(filter).FirstOrDefault();

            if (doc == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"The blog id {request.BlogId} does not exist!"));

            Blog.Blog blog = new Blog.Blog() { 
                AuthorId = doc.GetValue("author_id").AsString,
                Title = doc.GetValue("title").AsString,
                Content = doc.GetValue("content").AsString,
            };

            return new ReadBlogResponse() { Blog = blog };
        }
    }
}