using Blog;

var builder = WebApplication.CreateBuilder(args);

// gRPC
builder.Services.AddGrpcClient<BlogService.BlogServiceClient>(
    o => o.Address = new Uri(builder.Configuration.GetConnectionString("serverGRPC"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();