using ChatApp.Entities.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Consumer.Consumers
{
    public class ConnectConsumer : IConsumer<ConnectMessage>
    {
        public Task Consume(ConsumeContext<ConnectMessage> context)
        {
            Console.WriteLine($"{context.Message.Name} has joined the chat");
            return Task.CompletedTask;
        }
    }
}