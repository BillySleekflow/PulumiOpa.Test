using Microsoft.Extensions.Configuration;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.V20240301.Inputs;
using Pulumi.AzureNative.Insights.Inputs;
using Pulumi.AzureNative.Resources;
using Sleekflow.Infras.Components.Configs;
using Sleekflow.Infras.Constants;
using Sleekflow.Infras.Utils;
using App = Pulumi.AzureNative.App.V20240301;
using ContainerRegistry = Pulumi.AzureNative.ContainerRegistry;
using Docker = Pulumi.Docker;
using EventGrid = Pulumi.AzureNative.EventGrid;
using ScaleRuleArgs = Pulumi.AzureNative.App.V20240301.Inputs.ScaleRuleArgs;
using Storage = Pulumi.AzureNative.Storage;
using Web = Pulumi.AzureNative.Web;

namespace Sleekflow.Infras.Components.Opa;

public class Opa
{
    private readonly ContainerRegistry.Registry _registry;
    private readonly Output<string> _registryUsername;
    private readonly Output<string> _registryPassword;
    private readonly ResourceGroup _resourceGroup;
    private readonly List<ManagedEnvAndAppsTuple> _managedEnvAndAppsTuples;
    private readonly MyConfig _myConfig;

    private readonly string _blobContainerName = "opa-policies";

    public Opa(
        ContainerRegistry.Registry registry,
        Output<string> registryUsername,
        Output<string> registryPassword,
        ResourceGroup resourceGroup,
        List<ManagedEnvAndAppsTuple> managedEnvAndAppsTuples,
        MyConfig myConfig)
    {
        _registry = registry;
        _registryUsername = registryUsername;
        _registryPassword = registryPassword;
        _resourceGroup = resourceGroup;
        _managedEnvAndAppsTuples = managedEnvAndAppsTuples;
        _myConfig = myConfig;
    }

    public List<App.ContainerApp> InitOpa()
    {
        // Step 1: Create an Azure Resource Group
        // var resourceGroup = new ResourceGroup("tenant-hub-rg");
        var image = GetOpaImage("opa");

        #region Blob Storage

        // Step 2: Create an Azure Storage Account
        var storageAccount = new Storage.StorageAccount(
            "storageAccount",
            new Storage.StorageAccountArgs
            {
                ResourceGroupName = _resourceGroup.Name,
                Sku = new Storage.Inputs.SkuArgs
                {
                    Name = Storage.SkuName.Standard_LRS
                },
                Kind = Storage.Kind.StorageV2
            });

        // Step 3: Create a Blob Container for policy storage
        var policyStorage = new Storage.BlobContainer(
            "policiesContainer",
            new Storage.BlobContainerArgs
            {
                AccountName = storageAccount.Name,
                ResourceGroupName = _resourceGroup.Name,
                PublicAccess = Storage.PublicAccess.None,
                ContainerName = _blobContainerName
            },
            new CustomResourceOptions
            {
                Parent = storageAccount
            });

        #endregion

        #region Opa Server
        // Step 5: Push the OPA and Replicator images to ACR
        var imageName = Output.Format(
            $"{_registry.LoginServer}/opa:latest");
            // $"{_registry.LoginServer}/{ServiceNames.GetSleekflowPrefixedShortName(ServiceNames.OpenPolicyAgent)}:{_myConfig.BuildTime}");

        var opaImage = new Docker.Image(
            ServiceNames.GetSleekflowPrefixedShortName(ServiceNames.OpenPolicyAgent),
            new Docker.ImageArgs
            {
                ImageName = imageName,
                Build = new Docker.Inputs.DockerBuildArgs
                {
                    Dockerfile = "Dockerfile", // Path to your OPA Dockerfile
                },
                Registry = new Docker.Inputs.RegistryArgs
                {
                    Server = _registry.LoginServer,
                    Username = _registryUsername,
                    Password = _registryPassword
                }
            });

        /*
        var replicatorImage = new Docker.Image("replicator", new Docker.ImageArgs
        {
            ImageName = Output.Format($"{_registry.LoginServer}/replicator:latest"),
            Build = new Docker.Inputs.DockerBuildArgs
            {
                Dockerfile = "Dockerfile.replicator",  // Path to your Replicator Dockerfile
            },
            Registry = new Docker.Inputs.RegistryArgs
            {
                Server = _registry.LoginServer,
                Username = _registryUsername,
                Password = _registryPassword
            }
        }); */

        var apps = new List<App.ContainerApp>();
        foreach (var managedEnvAndAppsTuple in _managedEnvAndAppsTuples)
        {
            if (!managedEnvAndAppsTuple.IsExcludedFromManagedEnv(ServiceNames.OpenPolicyAgent))
            {
                continue;
            }

            var containerApps = managedEnvAndAppsTuple.ContainerApps;
            var managedEnvironment = managedEnvAndAppsTuple.ManagedEnvironment;

            // Step 6: Create a Container App Environment
            /*
            var containerEnv = new ManagedEnvironment(
                "containerEnv",
                new ManagedEnvironmentArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    Location = resourceGroup.Location,
                    AppLogsConfiguration = new App.Input.AppLogsConfigurationArgs
                    {
                        Destination = "log-analytics",
                        LogAnalyticsConfiguration = new LogAnalyticsConfigurationArgs
                        {
                            CustomerId = "your-log-analytics-customer-id"
                        }
                    }
                });
                */

            // Step 7: Deploy OPA container
            var opaApp = new App.ContainerApp(
                "sleekflow-opa-server",
                new App.ContainerAppArgs
                {
                    ResourceGroupName = _resourceGroup.Name,
                    ManagedEnvironmentId = managedEnvironment.Id,
                    Configuration = new App.Inputs.ConfigurationArgs
                    {
                        Ingress = new App.Inputs.IngressArgs
                        {
                            External = true, TargetPort = 8181
                        },
                        Registries = new App.Inputs.RegistryCredentialsArgs
                        {
                            Server = _registry.LoginServer,
                            Username = _registryUsername,
                            PasswordSecretRef = "registry-password"
                        }
                    },
                    Template = new App.Inputs.TemplateArgs
                    {
                        Scale = new App.Inputs.ScaleArgs()
                        {
                            MinReplicas = _myConfig.Name.ToLower() == "production" ? 4 : 1,
                            MaxReplicas = 10,
                            Rules = new List<ScaleRuleArgs>()
                            {
                                new App.Inputs.ScaleRuleArgs
                                {
                                    Name = "cpu",
                                    Custom = new App.Inputs.CustomScaleRuleArgs()
                                    {
                                        Type = "cpu",
                                        Metadata =
                                        {
                                            { "metricName", "cpu" },
                                            { "threshold", "70" },
                                            { "scaleType", "Rule" },
                                            { "operator", "GreaterThan" },
                                        },
                                        Auth =
                                        {
                                            new App.Inputs.ScaleRuleAuthArgs
                                            {
                                                SecretRef = "registry-password"
                                            }
                                        }
                                    }
                                },
                            }
                        },
                        Containers = new App.Inputs.ContainerArgs
                        {
                            Name = "sleekflow-opa-container",
                            Image = opaImage.BaseImageName,
                            Resources = new App.Inputs.ContainerResourcesArgs
                            {
                                Cpu = 0.5, Memory = "1.0Gi"
                            },
                            Env = EnvironmentVariablesUtils.GetDeduplicateEnvironmentVariables(
                                new List<App.Inputs.EnvironmentVarArgs>
                                {
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "ASPNETCORE_ENVIRONMENT", Value = "Production",
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "DOTNET_RUNNING_IN_CONTAINER", Value = "true",
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "ASPNETCORE_URLS", Value = "http://+:80",
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                                        Value = managedEnvAndAppsTuple.InsightsComponent.ConnectionString
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "SF_ENVIRONMENT",
                                        Value = managedEnvAndAppsTuple.FormatSfEnvironment()
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "SF_LOCATION",
                                        Value = LocationNames.GetAzureLocation(managedEnvAndAppsTuple.LocationName),
                                    },
                                    new App.Inputs.EnvironmentVarArgs
                                    {
                                        Name = "BLOB_STORAGE_URL",
                                        Value = Output.Format(
                                            $"https://{storageAccount.Name}.blob.core.windows.net/{policyStorage.Name}")
                                    },
                                    new App.Inputs.EnvironmentVarArgs()
                                    {
                                        Name = "OPA_DATA_PATH",
                                        Value = StorageUtils.GetConnectionString(_resourceGroup.Name, storageAccount.Name)
                                    }
                                }),
                            VolumeMounts = new InputList<App.Inputs.VolumeMountArgs>()
                            {
                                new App.Inputs.VolumeMountArgs
                                {
                                    VolumeName = "opa-data",
                                    MountPath = "/data"
                                }
                            }
                        },
                        Volumes = new InputList<App.Inputs.VolumeArgs>()
                        {
                            new App.Inputs.VolumeArgs
                            {
                                Name = "sleekflow-opa-data-volume",
                                StorageType = "AzureBlob",
                            }
                        }
                    }
                });

            // Step 8: Deploy Replicator container
            /*
            var replicatorApp = new App.ContainerApp(
                "sleekflow-opa-replicator",
                new App.ContainerAppArgs
                {
                    ResourceGroupName = _resourceGroup.Name,
                    ManagedEnvironmentId = managedEnvironment.Id,
                    Configuration = new App.Inputs.ConfigurationArgs
                    {
                        Ingress = new App.Inputs.IngressArgs
                        {
                            External = true, TargetPort = 8080
                        },
                        Registries = new App.Inputs.RegistryCredentialsArgs
                        {
                            Server = _registry.LoginServer,
                            Username = _registryUsername,
                            PasswordSecretRef = "registry-password"
                        }
                    },
                    Template = new App.Inputs.TemplateArgs
                    {
                        Containers = new App.Inputs.ContainerArgs
                        {
                            Name = "replicator-container",
                            Image = replicatorImage.BaseImageName,
                            Resources = new App.Inputs.ContainerResourcesArgs
                            {
                                Cpu = 0.5, Memory = "1.0Gi"
                            },
                            Env = new InputList<App.Inputs.EnvironmentVarArgs>
                            {
                                new App.Inputs.EnvironmentVarArgs
                                {
                                    Name = "BLOB_STORAGE_URL",
                                    Value = Output.Format(
                                        $"https://{storageAccount.Name}.blob.core.windows.net/{policyStorage.Name}")
                                },
                                new App.Inputs.EnvironmentVarArgs
                                {
                                    Name = "OPA_ENDPOINT", Value = opaApp.LatestRevisionFqdn
                                }
                            }
                        }
                    }
                });
                */

            #endregion

            #region Blob Trigger

            var appServicePlan = new Web.AppServicePlan(
                "sleekflow-opa-app-service-plan",
                new Web.AppServicePlanArgs
                {
                    ResourceGroupName = _resourceGroup.Name,
                    Kind = string.Empty,
                    Reserved = true,
                    Sku = new Web.Inputs.SkuDescriptionArgs
                    {
                        Tier = "Dynamic", Size = "Y1"
                    }
                });
            var webApp = new Web.WebApp(
                "sleekflow-opa-web-app-blob-trigger",
                new Web.WebAppArgs()
                {
                    ResourceGroupName = _resourceGroup.Name, ServerFarmId = appServicePlan.Id,
                });

            var blobUpdatedEventSubscription = new EventGrid.EventSubscription(
                "sleekflow-opa-blob-updated-event-subscription",
                new EventGrid.EventSubscriptionArgs
                {
                    Scope = policyStorage.Id,
                    EventSubscriptionName = "sleekflow-opa-blob-updated-event-subscription",
                    EventDeliverySchema = "EventGridSchema",
                    Destination = new EventGrid.Inputs.WebHookEventSubscriptionDestinationArgs
                    {
                        EndpointType = "WebHook",
                        EndpointUrl = Output.Format($"https://{webApp.DefaultHostName}/api/BlobTriggerFunction")
                    },
                    Filter = new EventGrid.Inputs.EventSubscriptionFilterArgs
                    {
                        IncludedEventTypes = new InputList<string>
                        {
                            "Microsoft.Storage.BlobCreated", "Microsoft.Storage.BlobDeleted"
                        },
                        IsSubjectCaseSensitive = false
                    }
                });

            #endregion

            containerApps.Add(ServiceNames.OpenPolicyAgent, opaApp);
            // containerApps.Add(ServiceNames.OpenPolicyAgent, replicatorApp);
            // apps.Add();
        }

        return apps;
    }

    private Docker.Image GetOpaImage(string imageName)
    {
        var myImageName = Output.Format($"{_registry.LoginServer}/{imageName}:{_myConfig.BuildTime}");
        var myImage = new Docker.Image(
            // ServiceNames.GetSleekflowPrefixedShortName(ServiceNames.OpenPolicyAgent),
            imageName,
            new Docker.ImageArgs
            {
                ImageName = imageName,
                Build = new Docker.Inputs.DockerBuildArgs
                {
                    Dockerfile = "Dockerfile", // Path to your OPA Dockerfile
                    CacheFrom = new Docker.Inputs.CacheFromArgs()
                    {
                       Images = new List<string>
                       {
                           "openpolicyagent/opa:latest"
                       }
                        // Output.Format($"{_registry.LoginServer}/{imageName}:latest")
                    },
                },
                Registry = new Docker.Inputs.RegistryArgs
                {
                    Server = _registry.LoginServer, Username = _registryUsername, Password = _registryPassword
                }
            });

        return myImage;
    }
}
