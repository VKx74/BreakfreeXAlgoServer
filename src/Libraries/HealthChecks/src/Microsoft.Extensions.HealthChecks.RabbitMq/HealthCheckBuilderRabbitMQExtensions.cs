using System;
using HealthCheckBuilderRabbitMQExtensions.Internal;
using EasyNetQ;

namespace Microsoft.Extensions.HealthChecks.RabbitMq
{
    public static class HealthCheckBuilderRabbitMqExtensions
    {
        public static HealthCheckBuilder AddRabbitMqCheck(this HealthCheckBuilder builder, string name, string rabbitMqConnectionString)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddRabbitMqCheck(builder, name, rabbitMqConnectionString, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddRabbitMqCheck(this HealthCheckBuilder builder, string name, string rabbitMqConnectionString, TimeSpan cacheDuration)
        {
            builder.AddCheck($"RabbitMqCheck({name})", () =>
            {
                try
                {
                    using (var bus = RabbitHutch.CreateBus(rabbitMqConnectionString))
                    {
                        if (bus.IsConnected)
                        {
                            return HealthCheckResult.Healthy($"RabbitMqCheck({name}): Healthy");
                        }
                    }
                    return HealthCheckResult.Unhealthy($"RabbitMqCheck({name}): Unhealthy");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"RabbitMqCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
