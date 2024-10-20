using Bogus;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Data.Entities;
using WebApplication2.Interfaces;
using WebApplication2.Mapper;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AndriyContexDb>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("MyConnectionDB")));
builder.Services.AddScoped<IImageWorker, ImageWorker>();
builder.Services.AddAutoMapper(typeof(AppMapperProfile));
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/{id?}");
using (var serviceScope = app.Services.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetService<AndriyContexDb>();
    var imageWorker = serviceScope.ServiceProvider.GetService<IImageWorker>();

    // Apply migrations if they are not applied
    context.Database.Migrate(); //автоматичний запуск міграцій на БД, якщо їх там немає

    if (!context.Categories.Any())
    {
        var imageName = imageWorker.Save("https://rivnepost.rv.ua/img/650/korisnoi-kovbasi-ne-buvae-hastroenterolohi-nazvali_20240612_4163.jpg");
        var kovbasa = new CategoryEntity
        {
            Name = "Ковбаси",
            Image = imageName,
            Description = "Тим часом відмовлятися від ковбаси повністю не обов’язково. " +
            "Важливо пам’ятати, що це делікатес, який можна вживати не більше 50 грамів на день."
        };

        imageName = imageWorker.Save("https://www.vsesmak.com.ua/sites/default/files/styles/large/public/field/image/syrnaya_gora_5330_1900_21.jpg?itok=RPUrRskl");
        var cheese = new CategoryEntity
        {
            Name = "Сири",
            Image = imageName,
            Description = "Cир – один з найчастіших гостей на нашому столі. " +
            "Адже це і смачно, і корисно, і доступно. Не можна сказати, що увесь, " +
            "що продається на прилавках супермаркетів твердий сир – неякісний."
        };

        imageName = imageWorker.Save("https://womanvip.ru/wp-content/uploads/2017/04/1-34.jpg");
        var bread = new CategoryEntity
        {
            Name = "Хліб",
            Image = imageName,
            Description = "У сегменті ринку «здорового харчування» існують сорти хліба, " +
            "які майже не сприяють набору зайвої ваги – наприклад, цільнозерновий хліб."
        };

        context.Categories.Add(kovbasa);
        context.Categories.Add(cheese);
        context.Categories.Add(bread);
        context.SaveChanges();
    }
    if (!context.Products.Any())
    {
        var categories = context.Categories.ToList();

        var fakerProduct = new Faker<ProductEntity>("uk")
                    .RuleFor(u => u.Name, (f, u) => f.Commerce.Product())
                    .RuleFor(u => u.Price, (f, u) => decimal.Parse(f.Commerce.Price()))
                    .RuleFor(u => u.Category, (f, u) => f.PickRandom(categories));

        string url = "https://picsum.photos/1200/800?product";

        var products = fakerProduct.GenerateLazy(30);

        Random r = new Random();

        foreach (var product in products)
        {
            context.Add(product);
            context.SaveChanges();
            int imageCount = r.Next(3, 5);
            for (int i = 0; i < imageCount; i++)
            {
                var imageName = imageWorker.Save(url);
                var imageProduct = new ProductImageEntity
                {
                    Product = product,
                    Image = imageName,
                    Priority = i
                };
                context.Add(imageProduct);
                context.SaveChanges();
            }
        }
    }
}

app.Run();
