---
services: Event-Hub
platforms: dotnet
author: yaohaizh
---

# Getting started on managing event hub, diagnostic settings and associated resources using C# #

      Azure Event Hub sample for managing event hub models.
        - Create a DocumentDB instance
        - Creates a Event Hub namespace and an Event Hub in it
        - Retrieve the root namespace authorization rule
        - Enable diagnostics on a existing cosmosDB to stream events to event hub


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/eventhub-dotnet-manage-event-hub-events.git

    cd eventhub-dotnet-manage-event-hub-events
  
    dotnet build
    
    bin\Debug\net452\ManageEventHubEvents.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.