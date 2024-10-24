using Pulumi;
using Pulumi.AzureNative.Resources;
using Sleekflow.Infras.Constants;
using App = Pulumi.AzureNative.App.V20240301;
using Insights = Pulumi.AzureNative.Insights.V20200202;
using OperationalInsights = Pulumi.AzureNative.OperationalInsights;

namespace Sleekflow.Infras.Components;

public class ManagedEnv
{
    private readonly ResourceGroup _resourceGroup;
    private readonly OperationalInsights.Workspace _logAnalyticsWorkspace;

    public ManagedEnv(ResourceGroup resourceGroup, OperationalInsights.Workspace logAnalyticsWorkspace)
    {
        _resourceGroup = resourceGroup;
        _logAnalyticsWorkspace = logAnalyticsWorkspace;
    }

    public App.ManagedEnvironment InitManagedEnv(string? name = null, string? locationName = null)
    {
        var workspaceSharedKeys = Output
            .Tuple(_resourceGroup.Name, _logAnalyticsWorkspace.Name)
            .Apply(
                items => OperationalInsights.GetSharedKeys.InvokeAsync(
                    new OperationalInsights.GetSharedKeysArgs
                    {
                        ResourceGroupName = items.Item1, WorkspaceName = items.Item2,
                    }));

        var managedEnv = new App.ManagedEnvironment(
            name == null
                ? $"sleekflow-container-apps-env"
                : $"sleekflow-container-apps-env-{name}",
            new App.ManagedEnvironmentArgs
            {
                ResourceGroupName = _resourceGroup.Name,
                Location =
                    locationName is not null
                        ? LocationNames.GetAzureLocation(locationName)
                        : _resourceGroup.Location,
                AppLogsConfiguration = new App.Inputs.AppLogsConfigurationArgs
                {
                    Destination = "log-analytics",
                    LogAnalyticsConfiguration = new App.Inputs.LogAnalyticsConfigurationArgs
                    {
                        CustomerId = _logAnalyticsWorkspace.CustomerId,
                        SharedKey = workspaceSharedKeys.Apply(r => r.PrimarySharedKey!)
                    }
                },
            },
            new CustomResourceOptions
            {
                Parent = _resourceGroup
            });

        return managedEnv;
    }
}