using AutoMapper;
using Dms.API.Services;
using Dms.DataDapper.Repositories;
using Dms.Domain.DbInfo;
using Dms.Domain.Mapper;
using Dms.Domain.Repositories;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddResponseCaching();
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
          options.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.AddResponseCompression(option =>
{
    option.EnableForHttps = true;
    option.Providers.Add<GzipCompressionProvider>();

});

builder.Services.AddSingleton(new DbInfo(builder.Configuration["ConnectionStrings:RDS_MySql_ConnectionSting"]));

builder.Services.AddTransient<IDataBaseRepository, DataBaseRepository>();
builder.Services.AddTransient<IGoogleDriveFilesRepository, GoogleDriveFilesRepository>();
builder.Services.AddTransient<IConfigurationRepository, ConfigurationRepository>();

var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile(new AutoMapperProfile());
});
var mapper=config.CreateMapper();
builder.Services.AddSingleton(mapper);


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(builder =>
               builder.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
