using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Entities.Messages;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace ChatApp.Publisher.Services
{
    public class PublishMessageService : BackgroundService
    {
        readonly IPublishEndpoint _publishEndpoint;
        bool _isFirstMessage = true;

        public PublishMessageService(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string name = await GetNameAsync();
            //if (_isFirstMessage)
            //{
            //    await SendMessageAsync(name, "");
            //    _isFirstMessage = false;
            //}
            while (!stoppingToken.IsCancellationRequested)
            {
                string messageText = await GetMessageAsync();
                await SendMessageAsync(name, messageText);
            }
        }

        private async Task<string> GetNameAsync()
        {
            await Console.Out.WriteLineAsync("Enter your name: ");
            string name = Console.ReadLine();
            ConnectMessage connectMessage = new() { Name = name };
            await _publishEndpoint.Publish<ConnectMessage>(connectMessage, x =>
            {
                x.SetRoutingKey("join-queue");
            });
            return name;
        }

        private async Task<string> GetMessageAsync()
        {
            await Console.Out.WriteLineAsync("Enter your message: ");
            string messageText = Console.ReadLine();
            return messageText;
        }

        private async Task SendMessageAsync(string name, string messageText)
        {
            ChatMessage message = new()
            {
                Name = name,
                Text = messageText
            };
            await _publishEndpoint.Publish<ChatMessage>(message, x =>
            {
                x.SetRoutingKey("message-queue");
            });
        }
    }
}
