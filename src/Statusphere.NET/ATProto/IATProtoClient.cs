namespace Statusphere.NET.ATProto;

public interface IATProtoClient
{
    /// <summary>
    /// Create an authentication session.
    /// </summary>
    /// <param name="identifier">Handle or other identifier supported by the server for the authenticating user.</param>
    /// <param name="password"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ATProtoSession> CreateSession(string identifier, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write a repository record, creating or updating it as needed. Requires auth, implemented by PDS.
    /// </summary>
    /// <param name="repo">The handle or DID of the repo (aka, current account).</param>
    /// <param name="collection">The NSID of the record collection.</param>
    /// <param name="rkey">The Record Key.</param>
    /// <param name="record">The record</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PutRecord<T>(string repo, string collection, string rkey, T record, CancellationToken cancellationToken = default) where T : ATRecord;
}