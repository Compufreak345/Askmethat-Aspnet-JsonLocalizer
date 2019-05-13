﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Askmethat.Aspnet.JsonLocalizer.TestSample
{
    public class Program
    {
#if NETCOREAPP1_1
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://*:5005;http://localhost:5006;http://MacbookAlex.local:5007")

                .Build();

            host.Run();
        }
#else
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
#endif
    }
}
