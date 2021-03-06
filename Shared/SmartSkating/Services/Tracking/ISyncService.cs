﻿using System;
using System.Threading.Tasks;
using Sanet.SmartSkating.Models.EventArgs;

namespace Sanet.SmartSkating.Services.Tracking
{
    public interface ISyncService
    {
        Task ConnectToHub(string sessionId);
        event EventHandler<WayPointEventArgs>? WayPointReceived;
        event EventHandler<SessionEventArgs>? SessionClosedReceived;
        Task CloseConnection();
    }
}