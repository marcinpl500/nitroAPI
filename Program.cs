using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Microsoft.Owin;
using System.Timers;

namespace linux2
{
    public class Program
    {
        private Timer t = new Timer(10000);
        private static  List<int> NitroBoostersIds;
        public DiscordSocketClient _client;
        private bool Debounce = false;
        private IServiceProvider _services;
        public static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public async Task RunBotAsync()
        {
            NitroBoostersIds = new List<int>();
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Error,
                DefaultRetryMode = RetryMode.AlwaysRetry

            });
            
            _services = new ServiceCollection()
               .AddSingleton(_client)
               
               .BuildServiceProvider();
            string tok = "";
            using(var reader = new StreamReader(@"/etc/web/a.txt")
                  {
                      tok = await reader.ReadLineAsync();
                  }

            var guilds = _client.Guilds.Count + 1;

            var rclient = _client.Rest;
            _client.LoggedIn += () => { Console.WriteLine("Bot Logged"); return Task.CompletedTask; };
            _client.LoggedOut += () => { Console.WriteLine("Bot logged out"); return Task.CompletedTask; };
            await _client.LoginAsync(TokenType.Bot, tok);
            await _client.SetStatusAsync(UserStatus.Offline);
            _client.Log += _client_Log;
            _client.Ready += async()=> 
            {   
                _client.Log += _client_Log;
                SocketGuild guild = _client.GetGuild(695337658553204777);
                var list = new List<IGuild>();
                list.Add(guild);
                Console.WriteLine($"Added guild to list {guild.Id}");
                try
                {
                    await _client.DownloadUsersAsync(list);
                    FetchBoosters().GetAwaiter().GetResult();
                  
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                t.Start();
                t.Elapsed += (object send, ElapsedEventArgs e) => { FetchBoosters().GetAwaiter().GetResult(); };
                Console.WriteLine("Bot Ready");
               
            };
            


          


            await _client.StartAsync();
            

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
       
        public Task FetchBoosters()
        {
            var sss = new List<int>();
            if(Debounce == false)
            {
                Debounce = true;
                Console.WriteLine("Starting fetching");
                
                SocketGuild guild = _client.GetGuild(695337658553204777);
                ulong NitroRole = 710500839588102164;
                Console.WriteLine("Getting nitro role");
                var role = guild.GetRole(NitroRole);
                var boosters = role.Members.ToList();
                Console.WriteLine(boosters.Count() + "Boosters amount");
                foreach (var k in boosters)
                {
                    Console.WriteLine($"Getting rover of {k.Id}");
                    var userData = CheckRoverDat(k.Id).Result;
                    sss.Add(userData.robloxId);
                }
                NitroBoostersIds = sss;
                Console.WriteLine(NitroBoostersIds.Count());
                Debounce = false;
                t.AutoReset = true;
                t.Start();
                return Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        
        public static List<int> Returned
        {
            get { return NitroBoostersIds; }
        }

        public async Task<RoverVerify> CheckRoverDat(object userId)
        {
            Console.WriteLine("Starting Rover");
            var h = new HttpClient();
            var VerifyList = new List<RoverVerify>();
            if (userId.GetType() == typeof(ulong))
            {
                string JSON = "";
                Uri Verify = new Uri($"https://verify.eryn.io/api/user/{userId}", UriKind.Absolute);
                var request = new HttpRequestMessage(HttpMethod.Get, Verify);
                var response = h.SendAsync(request);
                Console.WriteLine(response.Result.StatusCode.ToString());
                var content = response.Result.Content;
                var contentStream = await content.ReadAsStreamAsync();
                using (var reader = new StreamReader(contentStream))
                {
                    JSON = await reader.ReadToEndAsync();
                }
                var deserialized = JsonConvert.DeserializeObject<RoverVerify>(JSON);

                if (response.Result.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Found in ROVER");
                    VerifyList.Add(new RoverVerify()
                    {
                        Success = true,
                        status = deserialized.status,
                        robloxId = deserialized.robloxId,
                        robloxUsername = deserialized.robloxUsername
                    });

                }
                else
                {
                    Console.WriteLine(" Not found in ROVER");
                    VerifyList.Add(new RoverVerify()
                    {

                        Success = false,
                        error = deserialized.error,
                        errorCode = deserialized.errorCode
                    });

                }

            }
            return VerifyList.FirstOrDefault();
        }
    }
}
