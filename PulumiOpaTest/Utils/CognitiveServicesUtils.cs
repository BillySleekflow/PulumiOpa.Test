using Pulumi;
using Pulumi.AzureNative.CognitiveServices;
using Pulumi.AzureNative.Search;

namespace Sleekflow.Infras.Utils;

public static class CognitiveServicesUtils
{
    public static Output<string> GetCognitiveServicesAccountKey(Input<string> resourceGroupName, Input<string> accountName)
    {
        var accountKeys = ListAccountKeys.Invoke(
            new ListAccountKeysInvokeArgs
            {
                ResourceGroupName = resourceGroupName, AccountName = accountName
            });

        return accountKeys.Apply(keys => keys.Key1 ?? string.Empty);
    }

    public static Output<string> GetCognitiveSearchAdminKey(Input<string> resourceGroupName, Input<string> searchServiceName)
    {
        var adminKeys = ListAdminKey.Invoke(
            new ListAdminKeyInvokeArgs
            {
                ResourceGroupName = resourceGroupName, SearchServiceName = searchServiceName
            });

        return adminKeys.Apply(keys => keys.PrimaryKey);
    }
}