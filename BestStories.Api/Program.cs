using BestStories.Api.Services.Implementations;
using BestStories.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<IBestStoriesService, BestStoriesService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();