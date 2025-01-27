namespace Statusphere.NET;

public static class CarParser
{
    /// <summary>
    /// Read a protobuf-style varint from the given stream.
    /// Returns the integer value.
    /// </summary>
    public static ulong ReadVarint(Stream stream)
    {
        ulong result = 0;
        int shift = 0;

        while (true)
        {
            int b = stream.ReadByte();
            if (b < 0)
                throw new EndOfStreamException("Unexpected end of stream in varint.");

            // 7 bits of value, 1 bit of continuation
            ulong value = (ulong)(b & 0x7F);
            result |= value << shift;
            shift += 7;

            // If MSB is not set, we are done
            if ((b & 0x80) == 0)
                break;
        }

        return result;
    }

    /// <summary>
    /// Reads the CAR file from the byte[] and yields (CID, rawBlockData) pairs.
    /// </summary>
    public static IEnumerable<(string cid, byte[] data)> ReadCarBlocks(byte[] carBytes)
    {
        using var ms = new MemoryStream(carBytes);

        // 1) Read the header "section"
        ulong headerLen = ReadVarint(ms);
        if (headerLen > (ulong)ms.Length)
            throw new Exception("Invalid CAR header length.");

        byte[] headerBytes = new byte[headerLen];
        ms.Read(headerBytes, 0, (int)headerLen);

        // The header itself is DAG-CBOR-encoded. If you want to parse it, do:
        //   var cborHeader = CborDocument.Parse(headerBytes); 
        // and interpret it. Typically it has "roots" in an array, e.g. { "roots": [<CID>], "version": 1 }

        // 2) Now read each subsequent block
        while (ms.Position < ms.Length)
        {
            // Next varint = length of the next block section
            ulong blockLen = ReadVarint(ms);
            if (blockLen == 0)
            {
                // sometimes zero-length might appear at the end? 
                // break or continue as needed
                break;
            }

            byte[] blockBytes = new byte[blockLen];
            ms.Read(blockBytes, 0, (int)blockLen);

            // The block = [ CID-varint(s) ... , the remainder = the raw data block ]
            // We need to parse the front part as a CID. 
            // The CID itself can be variable length. 
            // We'll do a simple approach: read the CID from the front until we pass "multibase" + "multihash" info.

            // We'll parse the CID from 'blockBytes' and find where the actual data starts.
            var (cid, dataBytes) = ParseCidAndData(blockBytes);

            // Return that block
            yield return (cid, dataBytes);
        }
    }

    /// <summary>
    /// Very rough example of extracting the CID from the front of a block
    /// and returning the remainder as the block's data.
    /// 
    /// We'll produce a "string cid" for convenience,
    /// though in practice you may want to keep it as raw bytes or a CID object.
    /// </summary>
    private static (string cid, byte[] data) ParseCidAndData(byte[] blockBytes)
    {
        // 1) The first varint is the CID version & length info, but CIDs can be multiple varints in sequence:
        //    - For example, a typical v1 CID has:
        //         varint for version (e.g. 1)
        //         varint for the codec (e.g. dag-cbor = 113)
        //         the multihash, which itself has a varint prefix for hash code, varint for hash length, then the hash
        //    - A robust approach is to parse according to https://github.com/multiformats/cid
        // 
        // For brevity, here's a naive approach that reads a single "0x01" for version, 
        // then "0x71" for dag-cbor, then tries to read the rest as a multihash. 
        // 
        // In a real library you'd do a full parse. We'll just do enough so we can skip the correct number of bytes.

        // We'll do a naive approach:
        //   read varint => version
        //   read varint => codec
        //   read varint => multihash code
        //   read varint => multihash length
        //   read the next <multihash length> bytes = hash
        // Then reconstruct a "CID string" (this is not fully correct for all CIDs, but a rough example).
        // 
        // Because the range of possible varints is large, you'll want robust code in production. 
        // But let's illustrate:

        int offset = 0;

        // read varint => version
        (ulong cidVersion, int bytesUsed) = ReadVarintFromBytes(blockBytes, offset);
        offset += bytesUsed;

        // read varint => codec
        (ulong codec, int bytesUsed2) = ReadVarintFromBytes(blockBytes, offset);
        offset += bytesUsed2;

        // read varint => multihash code
        (ulong mhCode, int bytesUsed3) = ReadVarintFromBytes(blockBytes, offset);
        offset += bytesUsed3;

        // read varint => multihash length
        (ulong mhLength, int bytesUsed4) = ReadVarintFromBytes(blockBytes, offset);
        offset += bytesUsed4;

        // read next mhLength bytes = the hash
        byte[] mhHash = new byte[mhLength];
        Array.Copy(blockBytes, offset, mhHash, 0, (int)mhLength);
        offset += (int)mhLength;

        // Now offset is at the start of the actual block data
        byte[] dataBytes = new byte[blockBytes.Length - offset];
        Array.Copy(blockBytes, offset, dataBytes, 0, dataBytes.Length);

        // Construct a "human-friendly" CID string. 
        // The simplest is: "cidv1-{codec}-{mhCode}-{hexHash}" or use base58, etc.
        // For demonstration, do a naive approach:
        string cidStr = $"cidv{cidVersion}-codec{codec}-mh{mhCode}-" + BitConverter.ToString(mhHash).Replace("-", "");

        return (cidStr, dataBytes);
    }

    private static (ulong value, int bytesUsed) ReadVarintFromBytes(byte[] buffer, int startIndex)
    {
        ulong result = 0;
        int shift = 0;
        int index = startIndex;
        int bytesCount = 0;

        while (true)
        {
            if (index >= buffer.Length)
                throw new EndOfStreamException("Ran out of bytes while reading varint.");

            byte b = buffer[index];
            index++;
            bytesCount++;

            ulong val = (ulong)(b & 0x7F);
            result |= val << shift;
            shift += 7;

            if ((b & 0x80) == 0)
                break;
        }

        return (result, bytesCount);
    }
}