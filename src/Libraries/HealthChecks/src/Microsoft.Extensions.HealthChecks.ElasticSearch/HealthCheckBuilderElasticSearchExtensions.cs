using System;
using System.Net;
using ElasticsearchCRUD;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.HealthChecks.ElasticSearch
{
    public static class HealthCheckBuilderElasticSearchExtensions
    {
        private static readonly IElasticsearchMappingResolver ElasticsearchMappingResolver =
            new ElasticsearchMappingResolver();

        private const bool SaveChildObjectsAsWellAsParent = true;
        private const bool ProcessChildDocumentsAsSeparateChildIndex = true;
        private const bool UserDefinedRouting = true;

        private static readonly ElasticsearchSerializerConfiguration Config = new ElasticsearchSerializerConfiguration(
            ElasticsearchMappingResolver,
            SaveChildObjectsAsWellAsParent,
            ProcessChildDocumentsAsSeparateChildIndex,
            UserDefinedRouting);

        public static HealthCheckBuilder AddElasticSearchCheck(this HealthCheckBuilder builder,
            string name,
            IConfigurationRoot config,
            TimeSpan cacheDuration)
        {

            var connectionString = config["elcs"] ?? config.GetConnectionString("ElasticSearch");
            var username = config["elname"] ?? config.GetValue<string>("ElasticSearch:Username");
            var password = config["elpass"] ?? config.GetValue<string>("ElasticSearch:Password");

            builder.AddCheck($"ElasticSearchCheck({name})", () =>
            {
                try
                {
                    using (var elasticsearchContext = new ElasticsearchContext(connectionString, Config, new NetworkCredential(username, password)))
                    {
                        var exist = elasticsearchContext.AliasExists("user");
                    }
                    return HealthCheckResult.Healthy($"ElasticSearchCheck({name}): Healthy");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"ElasticSearchCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
