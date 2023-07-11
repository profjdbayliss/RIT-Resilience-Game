using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;

public struct TextureUV
{
    public string location;
    public int nameID;
    public float pixelStartX;
    public float pixelStartY;
    public float pixelStartX2;
    public float pixelEndY;
    public float pixelEndX;
    public float pixelStartY2;
    public float pixelEndX2;
    public float pixelEndY2;
    public int row;
    public int column;
}

public class TextureAtlas
{
    public static List<TextureUV> textureUVs;
    public static TextureAtlas instance = new TextureAtlas();
    public static readonly int pixelWidth = 128;
    public static readonly int pixelHeight = 128;
    public static int atlasHeight = 0;
    public static int atlasWidth = 0;
    public static Texture2D atlas;
    public static string[] names;

    public void CreateAtlasComponentData(string directoryName, string outputFileName)
    {
        // Get all file names in this directory
        names = Directory.GetFiles(directoryName);
        for (int i = 0; i < names.Length; i++)
        {
            UnityEngine.Debug.Log(names[i]);
        }

        // make the list of uvs
        textureUVs = new List<TextureUV>(names.Length);

        // Debug: make sure we see the files
        //foreach(string s in images)
        //{
        //    Debug.Log(s);
        // }

        // we're going to assume our images are a power of 2 so we just
        // need to get the sqrt of the number of images and round up
        int squareRoot = Mathf.CeilToInt(Mathf.Sqrt(names.Length));
        int squareRootH = squareRoot;
        atlasWidth = squareRoot * pixelWidth;
        atlasHeight = squareRootH * pixelHeight;

        if (squareRoot * (squareRoot - 1) > names.Length)
        {
            squareRootH = squareRootH - 1;
            atlasHeight = squareRootH * pixelHeight;
        }

        // allocate space for the atlas and file data
        atlas = new Texture2D(atlasWidth, atlasHeight);
        byte[][] fileData = new byte[names.Length][];

        // read the file data in parallel
        Parallel.For(0, names.Length,
            index =>
        {
            fileData[index] = File.ReadAllBytes(names[index]);
        });

        // Put all the images into the image file and write
        // all the texture data to the texture uv map list.
        int x1 = 0;
        int y1 = 0;
        Texture2D temp = new Texture2D(pixelWidth, pixelHeight);
        float pWidth = (float)pixelWidth;
        float pHeight = (float)pixelHeight;
        float aWidth = (float)atlas.width;
        float aHeight = (float)atlas.height;

        for (int i = 0; i < names.Length; i++)
        {
            float pixelStartX = ((x1 * pWidth) + 1) / aWidth;
            float pixelStartY = ((y1 * pHeight) + 1) / aHeight;
            float pixelEndX = ((x1 + 1) * pWidth - 1) / aWidth;
            float pixelEndY = ((y1 + 1) * pHeight - 1) / aHeight;
            TextureUV currentUVInfo = new TextureUV
            {
                location = names[i],
                nameID = i,
                pixelStartX = pixelStartX,
                pixelStartY = pixelStartY,
                pixelStartX2 = pixelStartX,
                pixelEndY = pixelEndY,
                pixelEndX = pixelEndX,
                pixelStartY2 = pixelStartY,
                pixelEndX2 = pixelEndX,
                pixelEndY2 = pixelEndY,
                row = y1,
                column = x1
            };
            //UnityEngine.Debug.Log(currentUVInfo.location);
            textureUVs.Add(currentUVInfo);

            //UnityEngine.Debug.Log(i);
            temp.LoadImage(fileData[i]);
            atlas.SetPixels(x1 * pixelWidth, y1 * pixelHeight, pixelWidth, pixelHeight, temp.GetPixels());

            x1 = (x1 + 1) % squareRoot;
            if (x1 == 0)
            {
                y1++;
            }


        }

        atlas.alphaIsTransparency = true;
        atlas.Apply();

        // write the atlas out to a file
        File.WriteAllBytes(outputFileName, atlas.EncodeToPNG());
    }

}
