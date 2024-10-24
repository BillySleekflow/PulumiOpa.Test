using Pulumi;

namespace Sleekflow.Infras;

internal static class Program
{
    static Task<int> Main() => Deployment.RunAsync<MyStack>();
}

// internal static class Program
// {
//     static async Task<int> Main()
//     {
//         var commerceHubSearch = new Sleekflow.Infras.Components.CommerceHub.CommerceHubSearch();
//
//         await commerceHubSearch.InitAzureSearchIndex();
//
//         return 1;
//     }
// }