
using System.Reflection;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Cli.Configuration;

public sealed class RuntimeConfiguration
{
    public AppOptions Options { get; }
    public string DataFilePath { get; }
    public string JsonStorePath { get; }
    public string? ApiKey { get; }

    private RuntimeConfiguration(
        AppOptions options,
        string dataFilePath,
        string? apiKey)
    {
        Options = options;
        DataFilePath = dataFilePath;
        JsonStorePath = Path.ChangeExtension(
            dataFilePath, ".json"
        );
        ApiKey = apiKey;
    }

    public static RuntimeConfiguration Load(string[] args)
    {
        string environment =
            Environment.GetEnvironmentVariable(
                "CROSSAPP_ENVIRONMENT"
            ) ?? "Development";

        string[] configArgs = args
            .Where(a =>
                a.StartsWith(
                    "--data-path=",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                a.StartsWith(
                    "--Storage:",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                a.StartsWith(
                    "--Orders:",
                    StringComparison.OrdinalIgnoreCase
                ))
            .Select(a =>
                a.StartsWith(
                    "--data-path=",
                    StringComparison.OrdinalIgnoreCase
                )
                    ? "--Storage:DataPath=" +
                      a["--data-path=".Length..]
                    : a)
            .ToArray();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false
            )
            .AddJsonFile(
                $"appsettings.{environment}.json",
                optional: true
            )
            .AddUserSecrets(
                Assembly.GetExecutingAssembly(),
                optional: true
            )
            .AddEnvironmentVariables(
                prefix: "CROSSAPP_"
            )
            .AddCommandLine(configArgs)
            .Build();

        var options = config.Get<AppOptions>()
            ?? new AppOptions();

        options.EnvironmentName = environment;

        bool commandLineHasPath = configArgs.Any(a =>
            a.StartsWith(
                "--Storage:DataPath=",
                StringComparison.OrdinalIgnoreCase
            )
        );

        string? legacyPath =
            Environment.GetEnvironmentVariable(
                "CROSSAPP_DATA_PATH"
            );

        if (!commandLineHasPath &&
            !string.IsNullOrEmpty(legacyPath))
        {
            options.Storage.DataPath = legacyPath;
        }

        AppOptionsValidator.Validate(options);

        string fullPath = options.Storage.ResolveFullPath(
            AppContext.BaseDirectory
        );

        return new RuntimeConfiguration(
            options,
            fullPath,
            config["Integration:ApiKey"]
        );
    }
}
