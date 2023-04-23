using RainDropWeb.Protocol;var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();
app.MapDefaultControllerRoute();
app.MapFallbackToFile("/", "index.html");
app.MapFallbackToFile("/Index", "index.html");
app.MapFallbackToFile("/Oscilloscope", "oscilloscope.html");

app.Run();