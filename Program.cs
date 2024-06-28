using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Konfigurace služby BlobServiceClient
builder.Services.AddSingleton(x => {
    string connectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
    return new BlobServiceClient(connectionString);
});

var app = builder.Build();

app.UseStaticFiles(); // Middleware pro servírování statických souborů z wwwroot

// Route pro poskytování souborů z Azure Blob Storage
app.MapGet("/blob/{fileName}", async (string fileName, BlobServiceClient blobServiceClient, HttpContext httpContext) =>
{
    var containerClient = blobServiceClient.GetBlobContainerClient("static-content");
    var blobClient = containerClient.GetBlobClient(fileName);

    if (await blobClient.ExistsAsync())
    {
        var blobDownloadInfo = await blobClient.DownloadAsync();
        httpContext.Response.ContentType = blobDownloadInfo.Value.ContentType;
        await blobDownloadInfo.Value.Content.CopyToAsync(httpContext.Response.Body);
    }
    else
    {
        httpContext.Response.StatusCode = 404;
    }
});

app.Run();
