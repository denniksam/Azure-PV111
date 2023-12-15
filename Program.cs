using Azure_PV111.Cron;
using Azure_PV111.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DataMiddleware>();

builder.Configuration.AddJsonFile("azuresettings.json");

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.UseMiddleware<DataMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

CronTask.Add(
    action: () => Console.WriteLine("CronTask 5"), 
    seconds: 5
);
CronTask.Add(() => Console.WriteLine("CronTask 3"), 3);
CronTask.Add(
    action: DataMiddleware.RemoveExpired,
    seconds: DataMiddleware.LifeTime
);
CronTask.Start();

app.Run();
