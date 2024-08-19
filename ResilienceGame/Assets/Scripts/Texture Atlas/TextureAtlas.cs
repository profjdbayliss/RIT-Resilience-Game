using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using System;

// basic information about where the texture is in the atlas
// NOTE: the row/col info is all that's needed in the csv file for the specific card
// for this game
public struct TextureLocation
{
    public string location;
    public int nameID;
    public int row;
    public int column;
}

// an atlas containing multiple card image textures
// of a specific size
public class TextureAtlas
{
    // the square size of the texture on one side
    // assumes all images are square!
    public const int SIZE = 256;
    public List<TextureLocation> mTextureLocations;
    public static TextureAtlas Instance = new TextureAtlas();
    public static readonly int PixelWidth = SIZE;
    public static readonly int PixelHeight = SIZE;
    public static int sAtlasHeight = 0;
    public static int sAtlasWidth = 0;
    public Texture2D mAtlas;
    public List<string> mNames;

    // create an atlas from an image directory
    // NOTE: all images need to be the same resolution AND
    // no other images should be in the dir
    public void CreateAtlasComponentData(string directoryName, string outputFileName)
    {
        // Get all file names in this directory
        directoryName = Application.streamingAssetsPath + "\\" + directoryName;
        mNames = new List<string>(Directory.GetFiles(directoryName));

        List<string> tempNames = new List<string>();
        for (int i = 0; i < mNames.Count; i++)
        {
            if (!mNames[i].Contains(".meta"))
            {
                tempNames.Add(mNames[i]);
            }
        }
        mNames = tempNames;

        // make the list of uvs
        mTextureLocations = new List<TextureLocation>(mNames.Count);


        // Debug: make sure we see the files
        //foreach(string s in images)
        //{
        //    Debug.Log(s);
        // }

        // we're going to assume our images are a power of 2 so we just
        // need to get the sqrt of the number of images and round up

        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(mNames.Count));
        int squareRootH = squareRoot;
        sAtlasWidth = squareRoot * PixelWidth;
        sAtlasHeight = squareRootH * PixelHeight;

        if (squareRoot * (squareRoot - 1) > mNames.Count)
        {
            squareRootH = squareRootH - 1;
            sAtlasHeight = squareRootH * PixelHeight;
        }

        // allocate space for the atlas and file data
        mAtlas = new Texture2D(sAtlasWidth, sAtlasHeight);
        byte[][] fileData = new byte[mNames.Count][];

        // read the file data in parallel
        Parallel.For(0, mNames.Count,
            index =>
        {
            if (mNames[index].Contains("meta") == false)
            {
                fileData[index] = File.ReadAllBytes(mNames[index]);
            }
        });

        // Put all the images into the image file and write
        // all the texture data to the texture uv map list.
        int x1 = 0;
        int y1 = 0;
        Texture2D temp = new Texture2D(PixelWidth, PixelHeight);
        float pWidth = (float)PixelWidth;
        float pHeight = (float)PixelHeight;
        float aWidth = (float)mAtlas.width;
        float aHeight = (float)mAtlas.height;

        for (int i = 0; i < mNames.Count; i++)
        {
            TextureLocation currentUVInfo = new TextureLocation
            {
                location = mNames[i],
                nameID = i,
                row = y1,
                column = x1
            };
            mTextureLocations.Add(currentUVInfo);

            //UnityEngine.Debug.Log(i);
            temp.LoadImage(fileData[i]);
            mAtlas.SetPixels(x1 * PixelWidth, y1 * PixelHeight, PixelWidth, PixelHeight, temp.GetPixels());

            x1 = (x1 + 1) % squareRoot;
            if (x1 == 0)
            {
                y1++;
            }


        }

        //atlas.alphaIsTransparency = true; // for images with transparency!
        mAtlas.Apply();

        // write the atlas out to a file: assume we want png
        File.WriteAllBytes(Application.streamingAssetsPath + "\\" + outputFileName, mAtlas.EncodeToPNG());
    }

    // Helpful when there are a group of images either in different directories or
    // interspersed with other images so that all images need to be read
    // separately. Helpful for listing images used for cards from the
    // CSV file.

    public void CreateAtlasFromFilenameList(string directoryName, string outputFileName, List<string> filenames)
    {


        // Get all file names in this directory
        directoryName = Application.streamingAssetsPath + "\\" + directoryName;
        mNames = filenames;

        // make the list of uvs
        mTextureLocations = new List<TextureLocation>(mNames.Count);

        // Debug: make sure we see the files
        //foreach(string s in images)
        //{
        //    Debug.Log(s);
        // }

        // we're going to assume our images are a power of 2 so we just
        // need to get the sqrt of the number of images and round up

        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(mNames.Count));
        int squareRootH = squareRoot;
        sAtlasWidth = squareRoot * PixelWidth;
        sAtlasHeight = squareRootH * PixelHeight;

        if (squareRoot * (squareRoot - 1) > mNames.Count)
        {
            squareRootH = squareRootH - 1;
            sAtlasHeight = squareRootH * PixelHeight;
        }

        // allocate space for the atlas and file data
        mAtlas = new Texture2D(sAtlasWidth, sAtlasHeight);
        byte[][] fileData = new byte[mNames.Count][];

        // read the file data in parallel
        Parallel.For(0, mNames.Count,
            index =>
            {
                if (!mNames[index].Equals(string.Empty) && !mNames[index].Equals("") )
                {
                    Debug.Log("names is : " + mNames[index] + " done.");
                    fileData[index] = File.ReadAllBytes(Application.streamingAssetsPath + "/" + mNames[index]);
                }
            });

        // Put all the images into the image file and write
        // all the texture data to the texture uv map list.
        int x1 = 0;
        int y1 = 0;
        Texture2D temp = new Texture2D(PixelWidth, PixelHeight);
        float pWidth = (float)PixelWidth;
        float pHeight = (float)PixelHeight;
        float aWidth = (float)mAtlas.width;
        float aHeight = (float)mAtlas.height;

        for (int i = 0; i < mNames.Count; i++)
        {

            if (!mNames[i].Equals(string.Empty) && !mNames[i].Equals(""))
            {
                TextureLocation currentUVInfo = new TextureLocation
                {
                    location = mNames[i],
                    nameID = i,
                    row = y1,
                    column = x1
                };
                Debug.Log("name: " + mNames[i] + " col: " + x1 + " row: " + y1);

                mTextureLocations.Add(currentUVInfo);

                //UnityEngine.Debug.Log(i);
                temp.LoadImage(fileData[i]);
                mAtlas.SetPixels(x1 * PixelWidth, y1 * PixelHeight, PixelWidth, PixelHeight, temp.GetPixels());

                x1 = (x1 + 1) % squareRoot;
                if (x1 == 0)
                {
                    y1++;
                }

            }
        }

        //atlas.alphaIsTransparency = true; // if there is any transparency in the images
        mAtlas.Apply();

        // write the atlas out to a file
        File.WriteAllBytes(Application.streamingAssetsPath + "\\" + outputFileName, mAtlas.EncodeToPNG());
    }
}
