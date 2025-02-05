# Statusphere.NET
This is a WIP .NET implementation of the [statusphere example app](https://github.com/bluesky-social/statusphere-example-app) which covers:

* Password authentication (todo: implement oauth)
* Fetch public user profile information
* Listen to the network firehose for new data
  * Enhancement from original: live UI updates via SignalR
* Publish data on the user's account using a custom schema

## Running Locally
```sh
git clone https://github.com/erinnmclaughlin/Statusphere.NET.git
cd src/Statusphere.NET
# if you need to install ef cli tools: 
# dotnet tool install dotnet-ef --global
dotnet ef database update
dotnet run
```


## Other Resources
* [AT Protocol Docs](https://atproto.com/)
* [Bluesky Docs](https://docs.bsky.app/)
* [Tagz App](https://github.com/FritzAndFriends/TagzApp) -- another Blazor-based AT Protocol app (heavily referenced when creating this example app)
