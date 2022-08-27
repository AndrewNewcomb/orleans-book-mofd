# orleans-book
Example code from the 'Microsoft Orleans for Developers' book. Published by Apress, Richard Astbury


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
If running locally you may get `curl: (60) SSL certificate problem` in which case add a `-k` parameter to disable the certificate check. Or fix the problem as specified for [Linux](https://curl.haxx.se/docs/sslcerts.html) or [Windows](https://curl.se/docs/sslcerts.html)

```
curl -X POST  \
-H 'Content-Type: application/json' \
--data-raw '{"instruction":"Tea please."}' \
'https://127.0.0.1:7055/robot/robbie/instruction'

curl https://127.0.0.1:7055/robot/robbie/instruction
```

## OrleansBook.Test
Test project that tests login in the GrainClasses. Uses an `ISiloConfigurator` to run a silo in memory.

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


# Problems / TODOs

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
