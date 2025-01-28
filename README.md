# Statusphere.NET
This is a WIP .NET implementation of the [statusphere example app](https://github.com/bluesky-social/statusphere-example-app) which covers:

* Password authentication (todo: implement oauth)
* Fetch public user profile information
* Listen to the network firehose for new data
  * Enhancement from original: live UI updates via SignalR
* Publish data on the user's account using a custom schema

## Running Locally
1. Clone this repository.
2. `cd src/Statusphere.NET`
3. `dotnet ef database update` (if you don't have EF CLI tools installed, run `dotnet tool install dotnet-ef --global`)
4. `dotnet run`
