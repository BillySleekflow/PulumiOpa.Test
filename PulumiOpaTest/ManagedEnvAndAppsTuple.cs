using Pulumi;
using Sleekflow.Infras.Components;
using Sleekflow.Infras.Constants;
using App = Pulumi.AzureNative.App.V20240301;
using Cache = Pulumi.AzureNative.Cache;
using Insights = Pulumi.AzureNative.Insights;
using OperationalInsights = Pulumi.AzureNative.OperationalInsights;
using Web = Pulumi.AzureNative.Web;

namespace Sleekflow.Infras;

public class ManagedEnvAndAppsTuple
{
    public App.ManagedEnvironment ManagedEnvironment { get; set; }

    public Dictionary<string, App.ContainerApp> ContainerApps { get; set; }

    public Dictionary<string, Web.WebApp> WorkerApps { get; set; }

    public Insights.Component InsightsComponent { get; set; }

    public OperationalInsights.Workspace LogAnalyticsWorkspace { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// Sleekflow.Infras.Constants.LocationNames
    /// </summary>
    public string LocationName { get; set; }

    // public MyServiceBus.ServiceBusOutput ServiceBus { get; set; }

    public Cache.Redis Redis { get; set; }

    public Cache.Redis SchedulerRedis { get; set; }

    // public MassTransitBlobStorage.MassTransitBlobStorageOutput MassTransitBlobStorage { get; set; }

    public ManagedEnvAndAppsTuple(
        App.ManagedEnvironment managedEnvironment,
        Dictionary<string, App.ContainerApp> containerApps,
        Dictionary<string, Web.WebApp> workerApps,
        Insights.Component insightsComponent,
        OperationalInsights.Workspace logAnalyticsWorkspace,
        string name,
        string locationName)
        // MyServiceBus.ServiceBusOutput serviceBus,
        // Cache.Redis redis,
        // Cache.Redis schedulerRedis,
        // MassTransitBlobStorage.MassTransitBlobStorageOutput massTransitBlobStorage)
    {
        ManagedEnvironment = managedEnvironment;
        ContainerApps = containerApps;
        WorkerApps = workerApps;
        InsightsComponent = insightsComponent;
        LogAnalyticsWorkspace = logAnalyticsWorkspace;
        Name = name;
        LocationName = locationName;
        // ServiceBus = serviceBus;
        // Redis = redis;
        // SchedulerRedis = schedulerRedis;
        // MassTransitBlobStorage = massTransitBlobStorage;
    }

    /// <summary>
    /// Determine if a service is part of this managed env or not.
    /// </summary>
    /// <param name="serviceName">The service name being tested.</param>
    /// <returns>Is part of this managed env or not.</returns>
    public virtual bool IsPartOfManagedEnv(string serviceName)
    {
        return true;
    }

    /// <summary>
    /// Determine if a service is excluded from this managed env or not.
    /// </summary>
    /// <param name="serviceName">The service name being tested.</param>
    /// <returns>Is excluded from this managed env or not.</returns>
    public virtual bool IsExcludedFromManagedEnv(string serviceName)
    {
        return false;
    }

    public virtual bool AreAllExcludedFromManagedEnv(params string[] serviceNames)
    {
        return false;
    }

    /// <summary>
    /// Obtain the container app name for a service in this managed env.
    /// </summary>
    /// <param name="appName">The service app name.</param>
    /// <returns>Container app name for given environment.</returns>
    public virtual string FormatContainerAppName(string appName)
    {
        var containerAppName = $"sleekflow-{appName}-app";
        return Name == "pri" ? containerAppName : $"{containerAppName}-{Name}";
    }

    /// <summary>
    /// Obtain the formatted managed env location for given container app and location.
    /// </summary>
    /// <returns>FormatSfEnvironment.</returns>
    public Output<string> FormatSfEnvironment()
    {
        return FormatSfEnvironment(Output.Create(LocationName));
    }

    /// <summary>
    /// Format the managed env location for SF_ENVIRONMENT.
    /// </summary>
    /// <param name="locationName">The location where the container app are hosted.</param>
    /// <returns>FormatSfEnvironment.</returns>
    protected virtual Output<string> FormatSfEnvironment(Output<string> locationName)
    {
        return Output.Tuple(ManagedEnvironment.Name, locationName).Apply(m => $"{m.Item1}_{m.Item2}");
    }

    /// <summary>
    /// Obtain the container app name for a service in this managed env.
    /// </summary>
    /// <param name="serviceName">e.g. ServiceNames.CrmHub.</param>
    /// <returns>e.g. Web.WebApp - CrmHubWorker.</returns>
    public Web.WebApp GetWorkerApp(string serviceName)
    {
        if (serviceName is
            ServiceNames.SalesforceIntegrator
            or ServiceNames.HubspotIntegrator
            or ServiceNames.Dynamics365Integrator)
        {
            return GetWorkerApp(ServiceNames.CrmHub);
        }

        return WorkerApps.Single(w => w.Key == serviceName)!.Value;
    }
}