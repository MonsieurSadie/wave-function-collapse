using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
  Right = 0,
  Down,
  Left,
  Up
}

public struct Constraint
{
  public int tile0;
  public int tile1;
  public Direction direction;
}

public class Cell
{
  public List<int> candidates = new List<int>();
}

public class WaveFunctionCollapse : MonoBehaviour
{
  public Sprite[] tiles;

  public List<Constraint> constraints = new List<Constraint>();

  Cell[] grid;
  public int gridWidth = 8;
  public int gridHeight = 8;
  
  Dictionary<string,int> spriteNameToID = new Dictionary<string, int>();

  public string constraintsFilename = "constraints.txt";

  void Awake()
  {
    LoadConstraints();
    SetupGrid();
    CreateGridDisplay();
  }

  // load a constraints file
  void LoadConstraints()
  {
    for (int i = 0; i < tiles.Length; i++)
    {
      spriteNameToID.Add(tiles[i].name, i);
    }

    string[] lines = System.IO.File.ReadAllLines(Application.dataPath + "/" + constraintsFilename);
    for (int i = 0; i < lines.Length; i++)
    {
      if(lines[i].StartsWith("//")) continue;

      string[] lineData = lines[i].Split(',');
      Constraint c = new Constraint();
      c.tile0 = int.Parse(lineData[0]);
      c.tile1 = int.Parse(lineData[1]);
      c.direction = (Direction)Direction.Parse(typeof(Direction), lineData[2]);

      constraints.Add(c);
    }
  }

  void SetupGrid()
  {
    grid = new Cell[gridWidth * gridHeight];
    for (int i = 0; i < grid.Length; i++)
    {
      grid[i] = new Cell();
      for (int n = 0; n < tiles.Length; n++)
      {
        grid[i].candidates.Add(n);
      }
    }
  }

  void Update()
  {
    if(Input.GetKeyDown(KeyCode.Space))
    {
      // get a candidate
      int nextGridCell = FindLowestEntropyCell();
      if(nextGridCell == -1)
      {
        Debug.Log("--- algorithm completed");
        // DisplayGrid();
        this.enabled = false;
        return;
      }

      // collapse
      int rndCandidateIdx = Random.Range(0, grid[nextGridCell].candidates.Count);
      int candidate = grid[nextGridCell].candidates[rndCandidateIdx];
      
      // update display
      for (int i = 0; i < grid[nextGridCell].candidates.Count; i++)
      {
        int c = grid[nextGridCell].candidates[i];
        if(c != candidate) RemoveFromCellDisplay(grid[nextGridCell], c);
      }

      grid[nextGridCell].candidates.Clear();
      grid[nextGridCell].candidates.Add(candidate);



      // propagate wave
      int cellY = Mathf.FloorToInt((float)nextGridCell / gridWidth);
      int cellX = nextGridCell - cellY * gridWidth;

      PropagateWave(new Vector2Int(cellX, cellY));

      Debug.Log("wave propagated");
    }
  }

  List<Vector2Int> exploredCells = new List<Vector2Int>();
  List<Vector2Int> explorationList = new List<Vector2Int>();
  void PropagateWave(Vector2Int cellPos)
  {
    explorationList.Clear();
    exploredCells.Clear();

    explorationList.Add(cellPos);

    while(explorationList.Count > 0)
    {
      Vector2Int cellToExplore = explorationList[0];
      if(!exploredCells.Contains(cellToExplore))
      {
        PropagateCellChangesToNeighbours(cellToExplore.x, cellToExplore.y);
        exploredCells.Add(cellToExplore);

        if(cellToExplore.x < gridWidth-1) explorationList.Add(cellToExplore + Vector2Int.right);
        if(cellToExplore.x > 0) explorationList.Add(cellToExplore - Vector2Int.right);
        if(cellToExplore.y < gridHeight-1) explorationList.Add(cellToExplore + Vector2Int.up);
        if(cellToExplore.y > 0) explorationList.Add(cellToExplore - Vector2Int.up);
      }
      explorationList.RemoveAt(0);
    }
  }

  void PropagateCellChangesToNeighbours(int cellX, int cellY)
  {
    if(cellX < 0 || cellY < 0 || cellX >= gridWidth || cellY >= gridHeight) return;

    int cellIdx = cellX + cellY * gridWidth;
    int numChanges = 0;
    if(cellX < gridWidth-1) numChanges += UpdateCellCandidates(grid[cellX + 1 + cellY*gridWidth ], grid[cellIdx], Direction.Right);
    if(cellY > 0) numChanges += UpdateCellCandidates(grid[cellX + (cellY-1)*gridWidth ], grid[cellIdx], Direction.Down);
    if(cellX > 0) numChanges += UpdateCellCandidates(grid[cellX-1 + cellY*gridWidth], grid[cellIdx], Direction.Left);
    if(cellY < gridHeight-1) numChanges += UpdateCellCandidates(grid[cellX + (cellY+1)*gridWidth], grid[cellIdx], Direction.Up);

    //return numChanges;
  }

  // by taking a neighbour and direction, reevaluate this cell candidates
  // by checking if a rule exist linking the candidates to the neighbour
  int UpdateCellCandidates(Cell cell, Cell neighbour, Direction dir)
  {
    int numChanges = 0;
    int i = 0;
    while(i < cell.candidates.Count && cell.candidates.Count > 1)
    {
      int candidate = cell.candidates[i];
      bool foundValideConstraint = false;
      for (int n = 0; n < neighbour.candidates.Count; n++)
      {
        if(IsValidConstraint(neighbour.candidates[n], candidate, dir))
        {
          foundValideConstraint = true;
          break;
        }  
      }
      if(!foundValideConstraint)
      {
        RemoveFromCellDisplay(cell, cell.candidates[i]);
        cell.candidates.RemoveAt(i);
        numChanges++;
      }
      else
      {
        i++;
      }
    }

    return numChanges;
  }

  bool IsValidConstraint(int tilea, int tileb, Direction dir)
  {
    for (int i = 0; i < constraints.Count; i++)
    {
      if( constraints[i].tile0 == tilea && 
          constraints[i].tile1 == tileb &&
          constraints[i].direction == dir)
      {
        return true;
      }
    }
    return false;
  }


  int FindLowestEntropyCell()
  {
    float lowestEntropy = Mathf.Infinity;
    int cellIdx = -1;

    for (int i = 0; i < grid.Length; i++)
    {
      if(grid[i].candidates.Count <= 1) continue;

      float entropy = grid[i].candidates.Count;
      if(entropy < lowestEntropy)
      {
        lowestEntropy = entropy;
        cellIdx = i;
      }
    }

    return cellIdx;
  }

  void PropagateWave()
  {

  }

  GameObject gridDisplay;
  void CreateGridDisplay()
  {
    float tileW = tiles[0].bounds.size.x;
    float tileH = tiles[0].bounds.size.y;

    Vector3 startCorner = new Vector3();
    startCorner.x = -(tileW * gridWidth) * 0.5f;
    startCorner.y = -(tileH * gridHeight) * 0.5f;

    gridDisplay = new GameObject("GridDisplay");

    for (int y = 0; y < gridHeight; y++)
    {
      for (int x = 0; x < gridWidth; x++)
      {
        GameObject root = new GameObject("" + x + "-" + y);
        root.transform.parent = gridDisplay.transform;

        for (int i = 0; i < tiles.Length; i++)
        {
          GameObject go = new GameObject();
          go.name = i.ToString();
          SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
          spr.sprite = tiles[ grid[y*gridWidth + x].candidates[i] ];
          go.transform.position = startCorner + Vector3.right * x * tileW + Vector3.up * y * tileH;
          go.transform.parent = root.transform;
        }
      }
    }
  }

  void RemoveFromCellDisplay(Cell cell, int tileid)
  {
    for (int i = 0; i < grid.Length; i++)
    {
      if(grid[i] == cell)
      {
        int x = i % gridWidth;
        int y = Mathf.FloorToInt( (float)i/gridWidth );
        GameObject o = GameObject.Find(""+x+"-"+y);
        Transform tile = o.transform.Find(""+tileid);
        tile.gameObject.SetActive(false);
      }
    }
  }


}
