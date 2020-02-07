using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetParser : MonoBehaviour
{
  public Sprite tileset;
  public int tileWidth = 8;
  public int tileHeight = 8;

  // marge d'égalité de couleur
  public float colorEpsilon = 0.1f;
  public float likenessMinPercent = 0.8f;

  public string outputFilename = "constraints.txt";
  string outputPath;

  Color[] tilesetData;
  int numTileX;
  int numTileY;
  int width;
  int height;

  string rules;
  
  [ContextMenu("Parse Tileset")]
  void ParseTilset()
  {
    outputPath = Application.dataPath + "/" + outputFilename + ".txt";
    rules = "";

    tilesetData = tileset.texture.GetPixels();

    // parses a tileset by comparing the borders of each tiles to find which ones can be neighbours
    width = tileset.texture.width;
    height = tileset.texture.height;

    numTileX = width / tileWidth;
    numTileY = height / tileHeight;

    for (int x = 0; x < numTileX; x++)
    {
      for (int y = 0; y < numTileY; y++)
      {

        FindMacthesForBorder(x, y, Direction.Left);
        FindMacthesForBorder(x, y, Direction.Right);
        FindMacthesForBorder(x, y, Direction.Down);
        FindMacthesForBorder(x, y, Direction.Up);
      }
    }

    System.IO.File.WriteAllText(outputPath, rules);
    Debug.Log("generated rules for " + outputFilename);
  }

  void FindMacthesForBorder(int tilex, int tiley, Direction dir)
  {
    bool foundMatch = false;
    for (int x = 0; x < numTileX; x++)
    {
      for (int y = 0; y < numTileY; y++)
      {
        if(ShareSameBorder(tilex, tiley, dir, x, y))
        {
          AddRule(tilex, tiley, x, y, dir.ToString());
          foundMatch = true;
        }
      }
    }

    if(!foundMatch)
    {
      Debug.Log("didn't find any match for " + tilex +"-"+tiley + " in direction " + dir.ToString());
    }
  }

  bool ShareSameBorder(int tile0x, int tile0y, Direction direction, int tile1x, int tile1y)
  {
    int id0 = tile0x + tile0y * numTileX;
    int id1 = tile1x + tile1y * numTileX;
    Debug.Log("comparing borders of tile " + id0 +" and tile " + id1 + " in direction " + direction);
    bool identical = false;

    int start0x = tile0x * tileWidth;
    int start0y = tile0y * tileHeight;
    int start1x = tile1x * tileWidth;
    int start1y = tile1y * tileHeight;

    int offset1 = 0;
    int offset2 = tileWidth-1;
    Color c1;
    Color c2;

    if(direction == Direction.Left || direction == Direction.Right)
    {
      offset1 = 0;
      offset2 = tileWidth-1;
      if(direction == Direction.Right)
      {
        offset1 = tileWidth-1;
        offset2 = 0;
      }

      float numEqualColors = 0;
      for (int y = 0; y < tileHeight; y++)
      {
        c1 = tilesetData[ start0x + offset1  + (start0y+y) * width ];
        c2 = tilesetData[ start1x + offset2  + (start1y+y) * width ];
        if(CompareColors(c1, c2) <= colorEpsilon)
        {
          numEqualColors++;
        }else
        {
          Debug.Log("pixel " + y + " doesn't match. diff is " + CompareColors(c1, c2));
        }
      }
      float colorDistance = numEqualColors / tileHeight;
      if(colorDistance >= likenessMinPercent)
      {
        identical = true;
      }

    }else
    {
      offset1 = 0;
      offset2 = tileHeight-1;
      if(direction == Direction.Up)
      {
        offset1 = tileHeight-1;
        offset2 = 0;
      }

      float numEqualColors = 0;
      for (int x = 0; x < tileWidth; x++)
      {
        c1 = tilesetData[ start0x + x + (start0y+offset1) * width ];
        c2 = tilesetData[ start1x + x + (start1y+offset2) * width ];
        if(CompareColors(c1, c2) < colorEpsilon) numEqualColors++;
      }
      float colorDistance = numEqualColors / tileHeight;
      if(colorDistance >= likenessMinPercent)
      {
        identical = true;
      }
    }

    return identical;
  }

  float CompareColors(Color c1, Color c2)
  {
    return  Mathf.Abs(c1.r - c2.r) +
            Mathf.Abs(c1.g - c2.g) +
            Mathf.Abs(c1.b - c2.b);
  }

  void AddRule(int x0, int y0, int x1, int y1, string dir)
  {
    // ids in sprite importer are from top-left...
    int id0 = ((numTileY-1) - y0)*numTileX + x0;
    int id1 = ((numTileY-1) - y1)*numTileX + x1;
    rules += id0 + "," + id1 + "," + dir + "\n";
  }
}
