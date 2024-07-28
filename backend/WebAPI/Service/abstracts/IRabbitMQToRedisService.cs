using System;

namespace WebAPI.Service.abstracts
{
    public interface IRabbitMQToRedisService : IDisposable
    {
        void StartListening();
    }
}