using ChatApp.Consumer.Consumers;
using ChatApp.Entities.Constants;
using MassTransit;
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(configurator =>
        {
            configurator.AddConsumer<ConnectConsumer>();
            configurator.AddConsumer<MessageConsumer>();


            configurator.UsingRabbitMq((context, _configurator) =>
            {
                _configurator.Host(BusConstants.Uri);

                _configurator.ReceiveEndpoint("join-queue", e => e.ConfigureConsumer<ConnectConsumer>(context));
                _configurator.ReceiveEndpoint("message-queue", e => e.ConfigureConsumer<MessageConsumer>(context));

            });
        });
    })
    .Build();

await host.RunAsync();