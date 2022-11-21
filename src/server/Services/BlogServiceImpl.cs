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
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(request.BlogId));
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

        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context) {
            var blogId = request.Blog.Id;
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = collection.Find(filter).FirstOrDefault();

            if(result == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"The blog id {blogId} does not exist!"));

            BsonDocument doc = new BsonDocument("author_id", request.Blog.AuthorId)
                .Add("title", request.Blog.Title)
                .Add("content", request.Blog.Content);

            collection.ReplaceOne(filter, doc);

            Blog.Blog blog = new Blog.Blog() { 
                AuthorId = doc.GetValue("author_id").AsString,
                Title = doc.GetValue("title").AsString,
                Content = doc.GetValue("content").AsString,
            };

            blog.Id = blogId;

            return new UpdateBlogResponse() { Blog = blog };
        }
        public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context) {
            var blogId = request.BlogId;
            
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = collection.DeleteOne(filter);

            if (result.DeletedCount == 0)
                throw new RpcException(new Status(StatusCode.NotFound, $"The blog id {blogId} does not exist!"));

            return new DeleteBlogResponse() { BlogId = blogId };
        }

        public override async Task ListBlog(ListBlogRequest request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context) {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;
            var list = collection.Find(filter);
            foreach (var item in list.ToList()) {
                await responseStream.WriteAsync(new ListBlogResponse() { Blog = new Blog.Blog() { 
                    Id = item.GetValue("_id").ToString(),
                    AuthorId = item.GetValue("author_id").AsString,
                    Content = item.GetValue("content").AsString,
                    Title = item.GetValue("title").AsString
                } });
            }
        }
    }
}