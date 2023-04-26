var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();
app.MapDefaultControllerRoute();
app.MapFallbackToFile("/", "index.html");
app.MapFallbackToFile("/Index", "index.html");
app.MapFallbackToFile("/Oscilloscope", "oscilloscope.html");
app.MapFallbackToFile("/Supply", "supply.html");

app.Run();