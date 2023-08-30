// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.CosmosDB;
using Azure.ResourceManager.CosmosDB.Models;
using Azure.ResourceManager.EventHubs;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;

namespace ManageEventHubEvents
{
    /**
     * Azure Event Hub sample for managing event hub models.
     *   - Create a DocumentDB instance
     *   - Creates a Event Hub namespace and an Event Hub in it
     *   - Retrieve the root namespace authorization rule
     *   - Enable diagnostics on a existing cosmosDB to stream events to event hub
     */
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        public static async Task RunSample(ArmClient client)
        {
            AzureLocation region = AzureLocation.EastUS;
            string rgName = Utilities.CreateRandomName("rgEvHb");
            string namespaceName = Utilities.CreateRandomName("ns");
            string eventHubName = "FirstEventHub";
            ResourceIdentifier? diagnosticSettingId = null;

            try
            {
                //============================================================
                // Create a resource group
                //
                var subscription = await client.GetDefaultSubscriptionAsync();
                var resourceGroupData = new ResourceGroupData(region);
                var resourceGroup = (await subscription.GetResourceGroups()
                    .CreateOrUpdateAsync(WaitUntil.Completed, rgName, resourceGroupData)).Value;
                _resourceGroupId = resourceGroup.Id;

                //=============================================================
                // Creates a Cosmos DB.
                //
                var locations = new List<CosmosDBAccountLocation>
                {
                    new CosmosDBAccountLocation()
                    {
                        LocationName = AzureLocation.WestUS,
                        IsZoneRedundant = false,
                        FailoverPriority = 0,
                    },
                    new CosmosDBAccountLocation()
                    {
                        LocationName = AzureLocation.SouthCentralUS,
                        IsZoneRedundant = false,
                        FailoverPriority = 1,
                    }
                };
                var cosmosData = new CosmosDBAccountCreateOrUpdateContent(region, locations)
                {
                    Kind = CosmosDBAccountKind.MongoDB,
                    ConsistencyPolicy = new ConsistencyPolicy(DefaultConsistencyLevel.Eventual)
                    {
                        MaxIntervalInSeconds = 0,
                        MaxStalenessPrefix = 0
                    }
                };
                var docDb = (await resourceGroup.GetCosmosDBAccounts()
                    .CreateOrUpdateAsync(WaitUntil.Completed, namespaceName, cosmosData)).Value;
                Utilities.Log("Created a DocumentDb instance with name: " + docDb.Data.Name);

                //=============================================================
                // Creates a Event Hub namespace and an Event Hub in it.
                //

                Utilities.Log("Creating event hub namespace and event hub");

                var eventHubsNamespaceData = new EventHubsNamespaceData(region);
                var ehNamespace = (await resourceGroup.GetEventHubsNamespaces()
                    .CreateOrUpdateAsync(WaitUntil.Completed, namespaceName, eventHubsNamespaceData)).Value;
                var eventHub = (await ehNamespace.GetEventHubs()
                    .CreateOrUpdateAsync(WaitUntil.Completed, eventHubName, new EventHubData())).Value;

                Utilities.Log($"Created event hub namespace {ehNamespace.Data.Name} and event hub {eventHub.Data.Name}");

                //=============================================================
                // Retrieve the root namespace authorization rule.
                //

                Utilities.Log("Retrieving the namespace authorization rule");

                var eventHubAuthRule = (await ehNamespace.GetEventHubsNamespaceAuthorizationRules()
                    .GetAsync("RootManageSharedAccessKey")).Value;

                Utilities.Log("Namespace authorization rule Retrieved");

                //=============================================================
                // Enable diagnostics on a cosmosDB to stream events to event hub
                //

                Utilities.Log("Enabling diagnostics events of a cosmosdb to stream to event hub");

                // Store Id of created Diagnostic settings only for clean-up
                var diagnosticSettingData = new DiagnosticSettingData()
                {
                    Metrics =
                    {
                        new MetricSettings(true)
                        {
                            Category = "AllMetrics",
                            TimeGrain = TimeSpan.FromMinutes(5),
                            RetentionPolicy = new RetentionPolicy(false, 0)
                        }
                    },
                    EventHubName = eventHubName,
                    EventHubAuthorizationRuleId = eventHubAuthRule.Id,
                    Logs =
                    {
                        new LogSettings(true)
                        {
                            Category = "DataPlaneRequests",
                            RetentionPolicy = new RetentionPolicy(false, 0)
                        },
                        new LogSettings(true)
                        {
                            Category = "MongoRequests",
                            RetentionPolicy = new RetentionPolicy(false, 0)
                        }
                    }
                };
                var ds = (await client.GetDiagnosticSettings(docDb.Id)
                    .CreateOrUpdateAsync(WaitUntil.Completed, "DiaEventHub", diagnosticSettingData)).Value;
                diagnosticSettingId = ds.Id;

                Utilities.Log("Streaming of diagnostics events to event hub is enabled");

                //=============================================================
                // Listen for events from event hub using Event Hub dataplane APIs.
            }
            finally
            {
                try
                {
                    if (diagnosticSettingId != null)
                    {
                        Utilities.Log("Deleting Diagnostic Setting: " + diagnosticSettingId);
                        await client.GetDiagnosticSettingResource(diagnosticSettingId).DeleteAsync(WaitUntil.Completed);
                        Console.WriteLine($"Deleted Diagnostic Setting: {diagnosticSettingId}");
                    }
                    if (_resourceGroupId is not null)
                    {
                        Console.WriteLine($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Console.WriteLine($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception ex)
                {
                    Utilities.Log(ex);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var credential = new DefaultAzureCredential();

                var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID");
                // you can also use `new ArmClient(credential)` here, and the default subscription will be the first subscription in your list of subscription
                var client = new ArmClient(credential, subscriptionId);

                await RunSample(client);
            }
            catch (Exception ex)
            {
                Utilities.Log(ex);
            }
        }
    }
}
