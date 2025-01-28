namespace Statusphere.NET.Client.Did;

public interface IDidClient
{
    Task<DidDocument> GetDidDocument(string did);
}