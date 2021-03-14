﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Sanet.SmartSkating.Dto;

namespace Sanet.SmartSkating.Services.Tracking
{
    public class SignalRService:ISyncService
    {
        private HubConnection? _connection;
        public async Task ConnectToHub(string accessToken, string url)
        {
            // _connection = new HubConnectionBuilder()
            //     .WithUrl($"{ApiNames.BaseUrl}/{sessionId}", (opts) =>
            //     {
            //         opts.Headers.Add("Ocp-Apim-Subscription-Key", ApiNames.AzureApiSubscriptionKey);
            //     })
            //     .Build();
            _connection = new HubConnectionBuilder()
                .WithUrl(url, (opts) =>
                {
                    opts.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .Build();
            _connection.On<string, string>("newWaypoint", (user, message) =>
            {
               Console.WriteLine($"SIGNALR MSG: {message}");
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"SIGNALR ERR: {e.Message}");
            }
            Console.WriteLine($"SIGNALR STT: {_connection.State}");
        }

        public async Task CloseConnection()
        {
            if (_connection != null) await _connection.StopAsync();
        }
    }
}