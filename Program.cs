// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MockSrvCliExamples;
using Newtonsoft.Json;
using Refit;
using RestEase;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using WireMock.Admin.Mappings;
using WireMock.Client;
using WireMock.Client.Builders;
using WireMock.Client.Extensions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMockServerClientExamples;

/*
1. docker pull sheyenrath/wiremock.net:1.6.5
2. docker run -p 9091:80 sheyenrath/wiremock.net
*/

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddRefitClient<ITestApiServer>(new RefitSettings
{
    ContentSerializer = new CustomContentSerializer(),
    Buffered = false,
})
    .RemoveAllLoggers()
    .ConfigureHttpClient(cfg =>
    {
        cfg.BaseAddress = new Uri("http://localhost:9091");
        cfg.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    });

using IHost host = builder.Build();

/* Mock с помощью подключения к удаленному mock-серверу wiremock */

var api = RestClient.For<IWireMockAdminApi>("http://localhost:9091", CustomRequestModifier.MyCustomRequestModifier);
await api.ResetMappingsAsync();
var mappingBuilder = api.GetMappingBuilder();

mappingBuilder.Given(m => m
    .WithTitle("MyTitle")
    .WithRequest(req => req        
        .UsingPost()
        .WithPath("/api/v1/GetTestResponse")
    )
    .WithResponse(rsp => rsp                
        .WithStatusCode(200)
        .WithHeaders(h => h.Add("Content-Type", "application/json; charset=utf-8"))
        .WithBodyAsJson(new TestResponse { HelloName = "test, default" })
    )
);

await mappingBuilder.BuildAndPostAsync();


/* Mock`s с помощью локального mock-сервера */
/*
// var server = WireMockServer.Start(9091);

server.Given(
        Request.Create().WithPath("/api/v1/GetTestResponse").UsingPost()
    )
    .RespondWith(
        Response.Create()
            .WithStatusCode(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithBody(new TestResponse { HelloName = "test, default" }.SerializeToJsonString())
    );
*/

var testApiClient = new TestApiClient(host.Services.GetRequiredService<ITestApiServer>());

var request1 = new TestRequest
{
    People = new List<People>
    {
        new People {Name = "Igor", Age = 30},
    }
};
var result1 = string.Empty;

try
{
    result1 = (await testApiClient.GetResponseAsync(request1))?.SerializeToJsonString();
}
catch (Refit.ApiException ex)
{
    result1 = ex.Message;
}

Console.WriteLine($"Request1: {request1.SerializeToJsonString()} Response1: {result1}");
Console.ReadLine();

// server.Stop();