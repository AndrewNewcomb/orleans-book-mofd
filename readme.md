# orleans-book
Example code from the 'Microsoft Orleans for Developers' book. Published by Apress, written by Richard Astbury


# Projects

## OrleansBook.GrainClasses 
Assembly that contains the business logic. 

## OrleansBook.GrainInterfaces
Assembly of interface definitions for use by clients. 

## OrleansBook.Host
Console app that hosts the silo. It uses the GrainClasses. 

## OrleansBook.Client
Console app that acts as a client and can create an instruction and get a count of instructions. Uses GrainInterfaces.

## OrleansBook.WebApi
WepApi that can GET the next instruction, and POST a new instruction. Uses GrainInterfaces.

Edge works, but shows https as not secure.
Cannot use Firefox as it doesn't trust the certificate.
Example usage in Postman collection `OrleansBook.postman_collection.json`

Example curl commands. 
Had a problem running locally with `curl: (60) SSL certificate problem` so disabled the certificate check with `-k`. Looks to be a proper fix as specified for [Linux](https://curl.haxx.se/docs/sslcerts.html) or [Windows](https://curl.se/docs/sslcerts.html)

```
curl -X POST  \
-k -H 'Content-Type: application/json' \
--data-raw '{"instruction":"Tea please."}' \
'https://127.0.0.1:7055/robot/robbie/instruction'

curl -k https://127.0.0.1:7055/robot/robbie/instruction

curl -X POST  \
-k -H 'Content-Type: application/json' \
--data-raw '{"hal":"Open the doors.", "mabel":"Biscuits please."}' \
'https://127.0.0.1:7055/batch'
```

## OrleansBook.Test
Test project that tests behaviour of the RobotGrain class. Uses an `ISiloConfigurator` to run a silo in memory.

# Persistence
OrleansBook.Host can be configured to use Memory, AzureBlob, AzureTable, or Postgresql persistence. When running locally, the relevant connection string will need to be set up in local secrets.
```
dotnet user-secrets set {key} {connection string}
```

The keys are
```
'ConnectionStrings:PostgresConnectionString'
'ConnectionStrings:AzureTableConnectionString'
'ConnectionStrings:AzureBlobConnectionString'
```

# Streams
OrleansBook.Host and OrleansBook.WebApi can be configured to use Memory or AzureQueue stream providers. Other stream providers are available but have not been tried.

If using AzureQueue its connection string needs setting as a user secret, with key 
```
'ConnectionStrings:AzureQueueConnectionString'
``` 

## Azure
If using Azure blob, table, or queue you can get the connection string from the Azure Portal.

## Postgresql
If using postgreq then a database will need creating and configuring, as [per the docs](https://dotnet.github.io/orleans/docs/host/configuration_guide/adonet_configuration.html). The SQL scripts in folder `./Postgres` will create a  database called `orleansbook`, set up the tables, roles, and a user called `orleansbookuser` (needs a password). 

The connection string will be in the format
```
host=127.0.0.1;database=orleansbook;username=orleansbookuser;password=???????
```


Useful commands for postgres
```
sudo service postgresql status
sudo service postgresql start
sudo service postgresql stop

sudo -u postgres psql orleansbook
```

# Orleans Dashboard
The instrumentation and stats packages, and their startup configuration are host OS specific. See comments in `OrleansBooh.Host/Program.cs`.

Dashboard visible at http://localhost:8080. See [Orleans dashboard readme](https://github.com/OrleansContrib/OrleansDashboard/blob/master/readme.md) for more information.


# Comments / Problems / TODOs

## Chapter 4 Debugging
- Setting log level from appsettings.json. It can be set from code, but how to set it from appsettings.json?

## Chapter 9 Streams
- Book has GetStreamProvider used from the RobotGrain constructor. 
  - This gave 'Unhandled exception. System.ArgumentException: Passing a half baked grain as an argument. It is possible that you instantiated a grain class explicitly, as a regular object and not via Orleans runtime or via proper test mocking'
  - Fixed it by using making the calls from within OnActivateAsync.
- Testing
  - How to test that it that RobotGrain publishes to the stream? 
  - How to test that the SubscriberGrain consumes messages? 
  - How to test that the StreamSubscriber consumes messages? 
  - Found some test code at https://github.com/dotnet/orleans/tree/main/test/Tester/StreamingTests

## Chapter 11 Transactions
- Find out more about Orleans' implementation of distributed transactions. Is it too good to be true?

## Chapter 12 Event Sourcing
Code as per the book but with the `robotStateStore` set to write to Azure Table Storage did write metadata to storage but didn't have any of the instructions state data. This means the queue of instructions is lost if the host restarts.

```json
{
    "$id": "1",
    "$type": "Orleans.EventSourcing.StateStorage.GrainStateWithMetaData`1[[OrleansBook.GrainClasses.EventSourcedState, OrleansBook.GrainClasses]], Orleans.EventSourcing",
    "State": {
        "$id": "2",
        "$type": "OrleansBook.GrainClasses.EventSourcedState, OrleansBook.GrainClasses",
        "Count": 1
    },
    "GlobalVersion": 5,
    "WriteVector": ",dev"
}
```

Added `builder.AddStateStorageBasedLogConsistencyProvider("EventStorage");` to the host configuration and the `[LogConsistencyProvider(ProviderName = "EventStorage")]` attribute on the `EventSourcedGrain`. It gave the same structure as above so looks to be the default.


Changed to `builder.AddLogStorageBasedLogConsistencyProvider("EventStorage");` which did result in the the event history being persisted.
You can see the `Apply` methods in the `EventSourcedState` class being called to load the state when the grain is started.

```json
{
    "$id": "1",
    "$type": "Orleans.EventSourcing.LogStorage.LogStateWithMetaData`1[[OrleansBook.GrainClasses.IEvent, OrleansBook.GrainClasses]], Orleans.EventSourcing",
    "Log": {
        "$type": "System.Collections.Generic.List`1[[OrleansBook.GrainClasses.IEvent, OrleansBook.GrainClasses]], System.Private.CoreLib",
        "$values": [{
                "$id": "2",
                "$type": "OrleansBook.GrainClasses.EnqueueEvent, OrleansBook.GrainClasses",
                "Value": "Tea please."
            }, {
                "$id": "3",
                "$type": "OrleansBook.GrainClasses.EnqueueEvent, OrleansBook.GrainClasses",
                "Value": "More tea please."
            }, {
                "$id": "4",
                "$type": "OrleansBook.GrainClasses.DequeueEvent, OrleansBook.GrainClasses",
                "Value": "Tea please."
            }, {
                "$id": "5",
                "$type": "OrleansBook.GrainClasses.DequeueEvent, OrleansBook.GrainClasses",
                "Value": "More tea please."
            }, {
                "$id": "6",
                "$type": "OrleansBook.GrainClasses.EnqueueEvent, OrleansBook.GrainClasses",
                "Value": "Yet more tea please."
            }
        ]
    },
    "GlobalVersion": 5,
    "WriteVector": ",dev"
}
```

There is also a `builder.AddCustomStorageBasedLogConsistencyProvider("EventStorage");` but I've not tried it.

## Chapter 14 Optimizations
Added a slow running method on the RobotGrain, but with a cancellation token so it can be cancelled.  
Exposed on the WebApi as `robot/{name}/doSomethingSlow/{slowTaskTimeSeconds}/{secondsToWaitBeforeCancelling}`
- To let the task complete: `curl -k https://127.0.0.1:7055/robot/robbie/doSomethingSlow/3/6`
- To cancel the task before completion: `curl -k https://127.0.0.1:7055/robot/robbie/doSomethingSlow/6/3`

## Chapter 15 Advanced features
IncomingGrainCallFilter
- Added system wide `MyIncomingGrainCallFilter` via the builder. It is applied to all grains including the grains that run the silo, so has code to check the grain namespace.
- Added an `IIncomingGrainCallFilter` implementation to `RobotGrain`, which is specific to that grain.

OutgoingGrainCallFilter
- Added system wide `MyOutgoingGrainCallFilter` via the builder.
- Didn't add any grain specific outgoing filters.

PlacementStrategy
- Override default placement strategy for `RobotGrain` with `Orleans.Placement.ActivationCountBasedPlacement` attribute.
- More info in the [grain-placement docs](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-placement)

StartUp
- Added `IStartupTask` to add an instruction for `RobotGrain` ROBBIE if its queue is empty.

Grainservice with GrainServiceClient
- `ExampleGrain` shows how to call a GrainServiceClient that then calls a GrainService.

Observer
- Found an example [ObserverManager](https://learn.microsoft.com/en-us/dotnet/orleans/grains/observers) in the Orleans docs.