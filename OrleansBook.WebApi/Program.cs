using Orleans;
using Orleans.Hosting;
using OrleansBook.GrainInterfaces;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var orleansClient = await ConnectToOrleans(builder.Configuration);      
        builder.Services.AddSingleton<IClusterClient>(orleansClient);

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
    }

    private static async Task<IClusterClient> ConnectToOrleans(ConfigurationManager configManager)
    {
        var client = new ClientBuilder()
            .UseLocalhostClustering()                        
            .AddSimpleMessageStreamProvider("SMSProvider")
            // .AddAzureQueueStreams("SMSProvider", configurator => 
            //     {
            //         configurator.ConfigureAzureQueue(
            //             ob => ob.Configure(options =>
            //             {
            //                 options.ConfigureQueueServiceClient(configManager.GetConnectionString("AzureQueueConnectionString"));
            //                 options.QueueNames = new List<string> { "orleans-stream-azurequeueprovider-0" };
            //             }));
            //     }
            // )
            .Build();

        await client.Connect();

        await client
            .GetStreamProvider("SMSProvider")
            .GetStream<InstructionMessage>(Guid.Empty, "StartingInstruction")
            .SubscribeAsync(new StreamSubscriber());

        return client;
    }
}