using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;

namespace Sleekflow.Infras.Utils;

public static class StorageUtils
{
    public static Output<string> SignedBlobReadUrl(
        Blob blob,
        BlobContainer container,
        StorageAccount account,
        ResourceGroup resourceGroup)
    {
        var serviceSasToken = ListStorageAccountServiceSAS
            .Invoke(
                new ListStorageAccountServiceSASInvokeArgs
                {
                    AccountName = account.Name,
                    Protocols = HttpProtocol.Https,

                    // TODO
                    SharedAccessStartTime = "2021-01-01",

                    // TODO
                    SharedAccessExpiryTime = "2030-01-01",
                    Resource = SignedResource.C,
                    ResourceGroupName = resourceGroup.Name,
                    Permissions = Permissions.R,
                    CanonicalizedResource = Output.Format($"/blob/{account.Name}/{container.Name}"),
                    ContentType = "application/zip",
                    CacheControl = "max-age=5",
                    ContentDisposition = "inline",
                    ContentEncoding = "deflate",
                })
            .Apply(blobSAS => blobSAS.ServiceSasToken);

        return Output.Format(
            $"https://{account.Name}.blob.core.windows.net/{container.Name}/{blob.Name}?{serviceSasToken}");
    }

    public static Output<string> GetConnectionString(Input<string> resourceGroupName, Input<string> accountName)
    {
        // Retrieve the primary storage account key.
        var storageAccountKeys = ListStorageAccountKeys.Invoke(
            new ListStorageAccountKeysInvokeArgs
            {
                ResourceGroupName = resourceGroupName, AccountName = accountName
            });

        return storageAccountKeys.Apply(
            keys =>
            {
                var primaryStorageKey = keys.Keys[0].Value;

                // Build the connection string to the storage account.
                return Output.Format(
                    $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={primaryStorageKey};EndpointSuffix=core.windows.net");
            });
    }
}