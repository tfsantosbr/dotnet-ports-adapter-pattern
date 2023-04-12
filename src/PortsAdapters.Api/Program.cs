using PortsAdapters.Application.FileStorage;
using PortsAdapters.ExternalServices.AwsS3Storage;
using PortsAdapters.ExternalServices.AzureBlobStorage;
using PortsAdapters.ExternalServices.GoogleCloudStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IFileStorage, AwsS3Storage>();
//builder.Services.AddTransient<IFileStorage, GoogleCloudStorage>();
//builder.Services.AddTransient<IFileStorage, AzureBlobStorage>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
