using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace MSI
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log)
        {
            _logger = log;
        }

        [FunctionName("TestMSI")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string accountName = Environment.GetEnvironmentVariable("SA_Name");
            string accountKey = Environment.GetEnvironmentVariable("SA_Key");
            string accountURL = "https://" + Environment.GetEnvironmentVariable("SA_Name") + ".blob.core.windows.net";            
           
            StorageSharedKeyCredential sk = new StorageSharedKeyCredential(accountName, accountKey);

            Uri uri = new Uri(accountURL);

            DataLakeServiceClient serviceclient = new DataLakeServiceClient(uri,sk);
            DataLakeFileSystemClient container =  serviceclient.GetFileSystemClient("test");
            DataLakeFileClient file = container.GetFileClient(name);

            return new OkObjectResult(file.OpenReadAsync().Result);
            //return new OkObjectResult(blob.DownloadTextAsync().Result);
        }
    }
}

//CloudStorageAccount account = CloudStorageAccount.Parse(Connection_String);
//CloudBlobClient client = account.CreateCloudBlobClient();
//CloudBlobContainer container = client.GetContainerReference("test");
//CloudBlockBlob blob = container.GetBlockBlobReference(name);