using Newtonsoft.Json;

namespace Sleekflow.Infras.Components.Configs;

public class MyConfig
{
    public string Name { get; }

    public bool IsCi { get; }

    public string BuildTime { get; }

    public IEnumerable<string> Origins { get; }

    public MyConfig()
    {
        IsCi = Environment.GetEnvironmentVariable("CI", EnvironmentVariableTarget.Process) == "true";

        var config = new Pulumi.Config("sleekflow");
        Name = config.Require("name");

        BuildTime = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");

        var clientsStr = config.Require("origins");
        Origins = JsonConvert.DeserializeObject<List<string>>(clientsStr)!;
    }
}