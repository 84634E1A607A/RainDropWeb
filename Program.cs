var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddRazorPages();
var app = builder.Build();

app.UseRouting();
app.UseStaticFiles();
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.MapFallbackToFile("/Oscilloscope", "oscilloscope.html");
app.MapFallbackToFile("/Supply", "supply.html");
app.MapFallbackToFile("/WaveGenerator", "wave-generator.html");

app.Run();