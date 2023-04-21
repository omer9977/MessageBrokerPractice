using ChatApp.Entities.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Consumer.Consumers
{
    public class MessageConsumer : IConsumer<ChatMessage>
    {
        public Task Consume(ConsumeContext<ChatMessage> context)
        {
            Console.WriteLine($"{context.Message.Name}: {context.Message.Text}");
            return Task.CompletedTask;
        }
    }
}
