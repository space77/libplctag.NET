﻿using libplctag;
using libplctag.DataTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpDotNetCore
{
    class ILoggerExample
    {
        public static void Run()
        {
            Host.CreateDefaultBuilder(null)
                .ConfigureServices(services =>
                {
                    services.AddLibPlcTagLogging();
                    services.AddHostedService<Example>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .Build()
                .Run();
        }

        class Example : IHostedService
        {
            public Task StartAsync(CancellationToken cancellationToken)
            {
                Task.Run(() => RunAsync());
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            async Task RunAsync()
            {
                var myTag = new Tag<DintPlcMapper, int>()
                {
                    Name = "MyTag[0]",
                    Gateway = "127.0.0.1",
                    Path = "1,0",
                    PlcType = PlcType.ControlLogix,
                    Protocol = Protocol.ab_eip,
                    Timeout = TimeSpan.FromSeconds(5),
                };

                await myTag.InitializeAsync();

                myTag.Value = 1234;
                await myTag.WriteAsync();
            }
        }
    }

    class LibPlcTagLogSource : IHostedService
    {
        private readonly ILogger<LibPlcTagLogSource> logger;

        public LibPlcTagLogSource(ILogger<LibPlcTagLogSource> logger)
        {
            this.logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            LibPlcTag.LogEvent += LibPlcTag_LogEvent;
            LibPlcTag.DebugLevel = DebugLevel.Spew;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            LibPlcTag.LogEvent -= LibPlcTag_LogEvent;
            return Task.CompletedTask;
        }

        private void LibPlcTag_LogEvent(object sender, LogEventArgs e)
        {
            var logLevel = e.DebugLevel switch
            {
                DebugLevel.None     => LogLevel.None,
                DebugLevel.Error    => LogLevel.Error,
                DebugLevel.Warn     => LogLevel.Warning,
                DebugLevel.Info     => LogLevel.Information,
                DebugLevel.Detail   => LogLevel.Debug,
                DebugLevel.Spew     => LogLevel.Trace,
                _                   => throw new NotImplementedException(),
            };
            logger.Log(logLevel, e.Message);
        }
    }

    static class LibPlcTagLoggingExtensions
    {
        public static IServiceCollection AddLibPlcTagLogging(this IServiceCollection services)
        {
            services.AddHostedService<LibPlcTagLogSource>();
            return services;
        }
    }
}
