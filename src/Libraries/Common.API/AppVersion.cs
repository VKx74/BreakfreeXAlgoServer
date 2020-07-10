using System;
using Microsoft.Extensions.PlatformAbstractions;

namespace Common.API
{
    public static class AppVersion
    {
        public static ApplicationEnvironment Application { get; } = PlatformServices.Default.Application;
        public static string RuntimeInfo { get; } = Application.RuntimeFramework.FullName;
        public static string ApiName { get; } = Application.ApplicationName;
        public static string ApiVersion { get; } = Application.ApplicationVersion;
        public static string FormattedApiVersion { get; } = " v" + Application.ApplicationVersion;

        public static void OutToConsole()
        {
            Console.WriteLine($"{ApiName}");
            Console.WriteLine($"Runtime: {RuntimeInfo}");
            Console.WriteLine($"API version: {ApiVersion}");
            Console.WriteLine();
        }
    }
}
