using Pulumi;
// using Sleekflow.Exceptions;
using App = Pulumi.AzureNative.App.V20240301;
using Web = Pulumi.AzureNative.Web;

namespace Sleekflow.Infras.Utils;

public static class EnvironmentVariablesUtils
{
    public static InputList<App.Inputs.EnvironmentVarArgs> GetDeduplicateEnvironmentVariables(
        List<App.Inputs.EnvironmentVarArgs> envVars)
    {
        var envVarNameOutputs = envVars.Select(envVar => envVar.Name.ToOutput()).ToList();

        var duplicateKeys = DetectDuplicate(envVarNameOutputs);
        if (duplicateKeys.Count != 0)
        {
            // throw new SfDuplicateEnvVarException(duplicateKeys);
            throw new Exception($"Duplicate Keys: {duplicateKeys}");
        }

        return envVars;
    }

    public static Web.Inputs.NameValuePairArgs[] GetDeduplicateNameValuePairs(
        Web.Inputs.NameValuePairArgs[] valuePairs)
    {
        var nameValuePairOutputs = valuePairs.Select(x => x.Name.ToOutput()).ToList();

        var duplicateKeys = DetectDuplicate(nameValuePairOutputs);
        if (duplicateKeys.Count != 0)
        {
            // throw new SfDuplicateEnvVarException(duplicateKeys);
            throw new Exception($"Duplicate Keys: {duplicateKeys}");
        }

        return valuePairs;
    }

    private static List<string> DetectDuplicate(List<Output<string>> outputLists)
    {
        var duplicateKeys = new List<string>();

        var existingKeys = new HashSet<string>();
        foreach (var output in outputLists)
        {
            output.Apply(
                keyName =>
                {
                    if (existingKeys.Add(keyName))
                    {
                        return keyName;
                    }

                    duplicateKeys.Add(keyName);

                    return keyName;
                });
        }

        return duplicateKeys;
    }
}