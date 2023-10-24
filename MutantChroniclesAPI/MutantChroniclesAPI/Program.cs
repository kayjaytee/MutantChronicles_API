using MongoDB.Driver.Core.Configuration;
using MutantChroniclesAPI.Model;
using System.Text.Json;
using MutantChroniclesAPI.Services.Data;
using MutantChroniclesAPI.Services;
using Microsoft.Extensions.Options;
using MutantChroniclesAPI.Model.CharacterModel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using MutantChroniclesAPI.Enums;
using System.Text.Json.Serialization;
using MutantChroniclesAPI.Converter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoDBSettings>(serviceProvider => serviceProvider.GetRequiredService<IOptions<MongoDBSettings>>().Value);
builder.Services.AddScoped<WeaponService>();

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
