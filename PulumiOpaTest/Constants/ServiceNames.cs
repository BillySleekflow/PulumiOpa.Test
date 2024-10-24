namespace Sleekflow.Infras.Constants;

public static class ServiceNames
{
    public const string AuditHub = "AuditHub";
    public const string ApiGateway = "ApiGateway";
    public const string CommerceHub = "CommerceHub";
    public const string CrmHub = "CrmHub";
    public const string SalesforceIntegrator = "SalesforceIntegrator";
    public const string SfmcJourneyBuilderCustomActivity = "SfmcJourneyBuilderCustomActivity";
    public const string HubspotIntegrator = "HubspotIntegrator";
    public const string Dynamics365Integrator = "Dynamics365Integrator";
    public const string EmailHub = "EmailHub";
    public const string FlowHub = "FlowHub";
    public const string FlowHubIntegrator = "FlowHubIntegrator";
    public const string MessagingHub = "MessagingHub";
    public const string IntelligentHub = "IntelligentHub";
    public const string PublicApiGateway = "PublicApiGateway";
    public const string ShareHub = "ShareHub";
    public const string TenantHub = "TenantHub";
    public const string WebhookHub = "WebhookHub";
    public const string UserEventHub = "UserEventHub";
    public const string SupportHub = "SupportHub";
    public const string TicketingHub = "TicketingHub";
    public const string Scheduler = "Scheduler";
    public const string InternalGateway = "InternalGateway";
    public const string OpenPolicyAgent = "OpenPolicyAgent";

    public static string GetShortName(string serviceName)
    {
        return serviceName switch
        {
            AuditHub => "ah",
            ApiGateway => "apigw",
            CommerceHub => "commh",
            CrmHub => "crm-hub",
            SalesforceIntegrator => "sf-in",
            SfmcJourneyBuilderCustomActivity => "sfmc-jb-ca",
            HubspotIntegrator => "hs-in",
            Dynamics365Integrator => "d365-in",
            EmailHub => "eh",
            FlowHub => "fh",
            FlowHubIntegrator => "fh-in",
            MessagingHub => "mh",
            IntelligentHub => "ih",
            PublicApiGateway => "pagw",
            ShareHub => "sh",
            TenantHub => "th",
            WebhookHub => "wh",
            UserEventHub => "ueh",
            SupportHub => "suph",
            TicketingHub => "tih",
            Scheduler => "sch",
            InternalGateway => "igw",
            OpenPolicyAgent => "opa",
            _ => throw new Exception("ServiceNames")
        };
    }

    public static string GetSleekflowPrefixedShortName(string serviceName)
    {
        return $"sleekflow-{GetShortName(serviceName)}";
    }

    public static string GetWorkerName(string serviceName)
    {
        return $"{serviceName}Worker";
    }
}