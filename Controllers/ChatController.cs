using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Api.Models;
using Chat.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace Chat.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ChatController : Controller
    {
        private readonly RabbitMQService rabbitMQService;
        private readonly RedisService chatConnectionService;
        private readonly ChatService chatService;

        private IConfiguration Configuration { get; }
        public ChatController(RabbitMQService rabbitMQService, IConfiguration configuration, RedisService chatConnectionService, ChatService chatService)
        {
            this.rabbitMQService = rabbitMQService;
            this.Configuration = configuration;
            this.chatConnectionService = chatConnectionService;
            this.chatService = chatService;
        }




        [HttpPost()]
        [Route("Chatters")]
        ///<summary>
        ///Get currently active chat user Ids
        /// </summary>
        public async Task<JsonResult> Chatters()
        {
            List<string> chat_clients = await chatConnectionService.GetConnectionIdList();

            return new JsonResult(new Response() { Data = new { chat_clients, myself = chatService.ConnectionId } });

        }

        [HttpPost()]
        [Route("SendMessage")]
        ///<summary>
        ///Send message to specific user 
        /// </summary>
        public async Task<JsonResult> SendMessage([FromQuery]string To, [FromQuery]string Message)
        {
            await chatService.SendMessage(To, Message);

            return new JsonResult(new Response() { Data = new { myself = chatService.ConnectionId } });

        }

        [HttpPost()]
        [Route("ReceivedMessages")]
        ///<summary>
        ///Retrive current user's received messages off of RabbitMQ
        /// </summary>
        public JsonResult ReceivedMessages()
        {
            return new JsonResult(new Response() { Data = rabbitMQService.GetReceivedMessageList(chatService.ConnectionId) });

        }

        [HttpPost()]
        [Route("SentMessages")]
        ///<summary>
        ///Retrive current user's sent messages off of RabbitMQ
        /// </summary>
        public JsonResult SentMessages()
        {
            return new JsonResult(new Response() { Data = rabbitMQService.GetSentMessageList(chatService.ConnectionId) });

        }


        [HttpGet()]
        [Route("HealthCheck")]
        ///<summary>
        ///Does helthcheck of the used components
        /// </summary>
        public ObjectResult HealthCheck()
        {
            bool checkStatus;

            #region Redis Healthcheck
            try
            {
                IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(Configuration["Redis"]);
                IDatabase db = multiplexer.GetDatabase();

                checkStatus = true;

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Redis: {ex.Message}");
            }



            #endregion

            #region RabbitMQ Check

            try
            {
                var factory = new ConnectionFactory() { HostName = Configuration["RabbitMQ"] };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                var consumer = new EventingBasicConsumer(channel);

                checkStatus = true;

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"RabbitMQ: {ex.Message}");
            }

            #endregion



            if (checkStatus)
            {
                return StatusCode(200,String.Empty);
            }
            else
            {
                return StatusCode(500,String.Empty);
            }


        }


    }
}