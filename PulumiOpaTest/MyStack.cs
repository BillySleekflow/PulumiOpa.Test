using Pulumi;
using Pulumi.AzureNative.Insights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Web;
using Sleekflow.Infras.Components;
using Sleekflow.Infras.Components.Configs;
using App = Pulumi.AzureNative.App.V20240301;
using AppConfiguration = Pulumi.AzureNative.AppConfiguration;
using ContainerRegistry = Pulumi.AzureNative.ContainerRegistry;
using OperationalInsights = Pulumi.AzureNative.OperationalInsights;
using ResourceGroup = Pulumi.AzureNative.Resources.ResourceGroup;
using Deployment = Pulumi.Deployment;
using Insights = Pulumi.AzureNative.Insights;

namespace Sleekflow.Infras;

public class MyStack : Stack
{
    public MyStack()
    {
        var myConfig = new MyConfig();
        
        var resourceGroup = new ResourceGroup("sleekflow-resource-group-" + myConfig.Name); 
        var (registry, adminUsername, adminPassword) = InitContainerRegistry(resourceGroup);

        var logAnalyticsWorkspace = new OperationalInsights.Workspace(
            "sleekflow",
            new OperationalInsights.WorkspaceArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new OperationalInsights.Inputs.WorkspaceSkuArgs
                {
                    Name = "PerGB2018"
                },
                RetentionInDays = 30,
            },
            new CustomResourceOptions
            {
                Parent = resourceGroup
            });
        
        // East Asia
        var managedEnvPri =
            new ManagedEnv(resourceGroup, logAnalyticsWorkspace)
                .InitManagedEnv();
        
        var containerAppManagedEnvAppInsights = new Insights.Component(
            $"sleekflow-container-apps-env-app-insight",
            new Insights.ComponentArgs
            {
                ResourceGroupName = resourceGroup.Name,
                ApplicationType = Insights.ApplicationType.Web,
                FlowType = "Redfield",
                RequestSource = "IbizaAIExtension",
                Kind = "Web",
                WorkspaceResourceId = logAnalyticsWorkspace.Id
            });

        var managedEnvAndAppsTuples = new List<ManagedEnvAndAppsTuple>()
        {
            new ManagedEnvAndAppsTuple(
                managedEnvPri,
                new Dictionary<string, App.ContainerApp>(),
                new Dictionary<string, WebApp>(),
                containerAppManagedEnvAppInsights,
                logAnalyticsWorkspace,
                "pri",
                "easeasia") // LocationNames.EastAsia,
                // serviceBusOutput,
                // redis,
                // schedulerRedis,
                // massTransitBlobStorage),
        };
    }
    
    private static (ContainerRegistry.Registry Registry, Output<string> AdminUsername, Output<string> AdminPassword)
        InitContainerRegistry(ResourceGroup resourceGroup)
    {
        var registry = new ContainerRegistry.Registry(
            "myregistry",
            new ContainerRegistry.RegistryArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Sku = new ContainerRegistry.Inputs.SkuArgs
                {
                    Name = ContainerRegistry.SkuName.Basic
                },
                AdminUserEnabled = true
            },
            new CustomResourceOptions
            {
                Parent = resourceGroup
            });

        var registryCredentials = ContainerRegistry.ListRegistryCredentials.Invoke(
            new ContainerRegistry.ListRegistryCredentialsInvokeArgs
            {
                ResourceGroupName = resourceGroup.Name, RegistryName = registry.Name
            });
        var adminUsername = registryCredentials.Apply(c => c.Username ?? string.Empty);
        var adminPassword =
            registryCredentials.Apply(c => Output.CreateSecret(c.Passwords.First().Value ?? string.Empty));
        return (registry, adminUsername, adminPassword);
    }
}