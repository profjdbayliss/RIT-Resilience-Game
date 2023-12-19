using UnityEngine;

public class CreateTextureAtlas : MonoBehaviour
{
    // Name of directory to get files from
    public string mDirectoryName = "blocks";
    public string mOutputFileName = "../atlas.png";
    public CardReader mReader;
    public void Start()
    {

        //UnityEngine.Debug.Log("Starting");

        //TextureAtlas.instance.CreateAtlasComponentData(mDirectoryName, mOutputFileName); // Not generating the atlas in build rn

        //UnityEngine.Debug.Log("Done with creation of texture atlas.");

        //mReader = GetComponent<CardReader>();
        //mReader.CSVRead();
    }

    public void DelayedStart()
    {
        UnityEngine.Debug.Log("Starting");

        TextureAtlas.instance.CreateAtlasComponentData(mDirectoryName, mOutputFileName); // Not generating the atlas in build rn

        UnityEngine.Debug.Log("Done with creation of texture atlas.");

        mReader = GetComponent<CardReader>();
        mReader.CSVRead();
    }
}
