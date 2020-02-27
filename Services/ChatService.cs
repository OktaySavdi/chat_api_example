using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Api.Services
{
    public class ChatService
    {
        HubConnection connection;
        private readonly ILogger<ChatService> logger;

        public ChatService(IConfiguration configuration,ILogger<ChatService> logger)
        {
            connection = new HubConnectionBuilder()
                .WithUrl(configuration["ChatAppUrl"])
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            this.logger = logger;
        }

        ///<summary>
        ///Connect to remote chat service
        /// </summary>
        public async Task StartConnection()
        {
            
            await connection.StartAsync();
            logger.LogInformation("Connection to Chat Service has been established via {0} connection",connection.ConnectionId);
        }

        ///<summary>
        ///Sends real-time signalr message to specific user
        /// </summary>
        public async Task SendMessage(string To, string Message)
        {
            try
            {
                if (String.IsNullOrEmpty(To))
                {
                    await connection.InvokeAsync("SendMessageToAll", Message);
                
                }
                else
                {
                    await connection.InvokeAsync("SendMessageToUser", To, Message);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        ///<summary>
        ///Gets current signalr connection id
        /// </summary>
        public string ConnectionId
        {
            get
            {
                return connection.ConnectionId;
            }

        }

    }
}
