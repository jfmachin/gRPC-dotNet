using Blog;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using static Blog.BlogService;

namespace client.API.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class BlogController : ControllerBase {
        private readonly ILogger<BlogController> logger;
        private readonly BlogServiceClient blogServiceClient;

        public BlogController(ILogger<BlogController> logger, BlogServiceClient blogServiceClient) {
            this.logger = logger;
            this.blogServiceClient = blogServiceClient;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create() {
            var response = blogServiceClient.CreateBlog(new CreateBlogRequest() { 
                Blog = new Blog.Blog() { 
                    AuthorId = "jmachin",
                    Title = "My new blog!",
                    Content = "Hello world"
                }
            });

            logger.LogInformation($"The blog {response.Blog.Id} was created!");
            return Ok();
        }

        [HttpGet("read")]
        public async Task<ActionResult> Read() {
            var id = "63706e3fa64db2aeefecb000";
            logger.LogInformation($"Reading blog id {id} !");

            var response = blogServiceClient.ReadBlog(new ReadBlogRequest() { BlogId = id });
            return Ok(new ReadBlogResponse() { Blog = response.Blog });
        }

        [HttpPut("update")]
        public async Task<ActionResult> Update() {
            Blog.Blog blog = new Blog.Blog() { 
                Id = "63706e3fa64db2aeefecb000",
                AuthorId = "updated author",
                Title = "updated title!",
                Content = "updated content"
            };

            var response = blogServiceClient.UpdateBlog(new UpdateBlogRequest() { Blog = blog });
            logger.LogInformation($"Updated blog {response.Blog} !");
            return Ok(new UpdateBlogResponse() { Blog = response.Blog });
        }

        [HttpPut("delete")]
        public async Task<ActionResult> Delete() {
            try {
                var id = "63706e3fa64db2aeefecb000";
                var response = blogServiceClient.DeleteBlog(new DeleteBlogRequest() { BlogId = id });
                logger.LogInformation($"Deleted blog {response.BlogId} !");
                return Ok(new DeleteBlogResponse() { BlogId = id });
            }
            catch (RpcException e) {
                logger.LogError(e.Status.Detail);
                return BadRequest();
            }
        }

        [HttpPut("list")]
        public async Task<ActionResult> List() {
            var response = blogServiceClient.ListBlog(new ListBlogRequest());

            var list = new List<ListBlogResponse>();
            while (await response.ResponseStream.MoveNext()) { 
                logger.LogInformation($"{response.ResponseStream.Current}");
                list.Add(response.ResponseStream.Current);
            }

            return Ok(list);
        }
    }
}
