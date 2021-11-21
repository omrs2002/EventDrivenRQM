using EventDrivenRQM.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PostService.Data;
using PostService.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;

namespace PostService
{
    public class Program
    {
         //public static IOptions<RabitMQSettings> _appSettings;

        public static void Main
            (
            string[] args//,            IOptions<RabitMQSettings> appSettings
            )
        {
            //_appSettings = appSettings;
            ListenForIntegrationEvents();
            CreateHostBuilder(args).Build().Run();
        }

        private static void ListenForIntegrationEvents()
        {

            //var factory = new ConnectionFactory()
            //{
            //    HostName = _appSettings.Value.HostName,
            //    UserName = _appSettings.Value.UserName,
            //    Password = _appSettings.Value.Password
            //};


            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var contextOptions = new DbContextOptionsBuilder<PostServiceContext>()
                    .UseSqlite(@"Data Source=post.db")
                    .Options;
                var dbContext = new PostServiceContext(contextOptions);

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                var data = JObject.Parse(message);
                var type = ea.RoutingKey;
                if (type == "user.add")
                {
                    dbContext.User.Add(new User()
                    {
                        ID = data["id"].Value<int>(),
                        Name = data["name"].Value<string>()
                    });
                    dbContext.SaveChanges();
                }
                else if (type == "user.update")
                {
                    var user = dbContext.User.First(a => a.ID == data["id"].Value<int>());
                    user.Name = data["newname"].Value<string>();
                    dbContext.SaveChanges();
                }
            };
            channel.BasicConsume(queue: "user.postservice",
                                     autoAck: true,
                                     consumer: consumer);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
