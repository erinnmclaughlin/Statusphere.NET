This is a WIP .NET implementation of the [statusphere example app](https://github.com/bluesky-social/statusphere-example-app) which covers:

* Password authentication (todo: implement oauth)
* Fetch public user profile information
* Listen to the network firehose for new data
  * Enhancement from original: live UI updates via SignalR
* Publish data on the user's account using a custom schema
