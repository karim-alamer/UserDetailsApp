using iText.Bouncycastleconnector;
using iText.Commons.Bouncycastle;

var builder = WebApplication.CreateBuilder(args);

// Initialize BouncyCastle
IBouncyCastleFactory factory = BouncyCastleFactoryCreator.GetFactory();
factory.CreateIDigest("SHA-256");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // Add HttpClient support

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();