using Microsoft.Extensions.Options;
using MutantChroniclesAPI.Converter;
using MutantChroniclesAPI.Services;
using MutantChroniclesAPI.Services.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoDBSettings>(serviceProvider => serviceProvider.GetRequiredService<IOptions<MongoDBSettings>>().Value);
builder.Services.AddScoped<WeaponService>();
builder.Services.AddScoped<EnviromentService>();

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new StringEnumConverterWithDescription());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();
