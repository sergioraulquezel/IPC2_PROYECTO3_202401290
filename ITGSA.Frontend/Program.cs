using ITGSA.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient("API", client =>
{
    var url = builder.Configuration["ApiUrl"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(url);
});
builder.Services.AddScoped<ApiClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
