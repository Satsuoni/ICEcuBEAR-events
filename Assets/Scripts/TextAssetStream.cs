using AudioSynthesis;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class TextAssetStream : IResource
{
    [SerializeField]
    TextAsset byteStream;
    [SerializeField]
    string name;
    // Start is called before the first frame update
    public bool ReadAllowed()
    {
        return true;
    }

    public bool WriteAllowed()
    {
        return false;
    }

    public bool DeleteAllowed()
    {
        return false;
    }

    public string GetName()
    {
        return name;
    }

    public Stream OpenResourceForRead()
    {
        return new MemoryStream(byteStream.bytes);
    }

    public Stream OpenResourceForWrite()
    {
        throw new System.NotImplementedException();
    }

    public void DeleteResource()
    {
        throw new System.NotImplementedException();
    }
}


