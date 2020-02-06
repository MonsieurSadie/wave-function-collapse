using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesetParser : MonoBehaviour
{
  public Sprite tileset;
  public int tileWidth = 8;
  public int tileHeight = 8;

  public string outputFilename = "constraints.txt";
  public int lastSpriteID = 0;
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
    for (int x = 0; x < numTileX; x++)
    {
      for (int y = 0; y < numTileY; y++)
      {
        if(x == tilex && y == tiley) continue;

        if(ShareSameBorder(tilex, tiley, dir, x, y))
        {
          AddRule(tilex + tiley * numTileX, x + y * numTileX, dir.ToString());
        }
      }
    }
  }

  bool ShareSameBorder(int tilex, int tiley, Direction direction, int otherx, int othery)
  {
    bool identical = true;

    int startx = tilex * tileWidth;
    int starty = tiley * tileHeight;

    int offset1 = 0;
    int offset2 = tileWidth;
    Color c1;
    Color c2;

    if(direction == Direction.Left || direction == Direction.Right)
    {
      offset1 = 0;
      offset2 = tileWidth;
      if(direction == Direction.Right)
      {
        offset1 = tileWidth-1;
        offset2 = 0;
      }
      for (int y = 0; y < tileHeight; y++)
      {
        c1 = tilesetData[ startx + offset1  + (starty+y) * width ];
        c2 = tilesetData[ startx + offset2  + (starty+y) * width ];
        if(c1 != c2)
        {
          identical = false;
          break;
        }
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
      for (int x = 0; x < tileWidth; x++)
      {
        c1 = tilesetData[ startx + x + (starty+offset1) * width ];
        c2 = tilesetData[ startx + x + (starty+offset2) * width ];
        if(c1 != c2)
        {
          identical = false;
          break;
        }
      }
    }

    return identical;
  }

  void AddRule(int id0, int id1, string dir)
  {
    // ids in sprite importer are from top-left...
    rules += (lastSpriteID-id0)+","+(lastSpriteID-id1)+","+dir+"\n";
  }
}
