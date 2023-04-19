var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.MapDefaultControllerRoute();
app.MapGet("/", () => "Hello World!");

app.Run();