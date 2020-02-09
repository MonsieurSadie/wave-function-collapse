// #define STEP_BY_STEP

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
  public string name;
  public int x;
  public int y;
  public Cell left  = null;
  public Cell right = null;
  public Cell up    = null;
  public Cell down  = null;
}

public class WaveFunctionCollapse : MonoBehaviour
{
  public Sprite[] tiles;

  public int[] usedTiles;

  public int[] tilesWeights;

  public List<Constraint> constraints = new List<Constraint>();

  Cell[] grid;
  public int gridWidth = 8;
  public int gridHeight = 8;

  public string constraintsFilename = "constraints.txt";

  Dictionary<int, List<Constraint>> rulesByStartID;

  void Awake()
  {
    LoadConstraints();
    SetupGrid();
    CreateGridDisplay();
  }

  public void OnRunAlgorithm()
  {
    StartCoroutine(RunAlgorithm());
  }

  // load a constraints file
  void LoadConstraints()
  {
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


    rulesByStartID = new Dictionary<int, List<Constraint>>();
    for (int i = 0; i < constraints.Count; i++)
    {
      int tileid = constraints[i].tile0;
      if(!rulesByStartID.ContainsKey(tileid))
      {
        rulesByStartID.Add(tileid, new List<Constraint>());
      }
      rulesByStartID[tileid].Add(constraints[i]);
    }
  }

  void SetupGrid()
  {
    // if a subset of tiles is not provided by user, we use all of them
    if(usedTiles.Length == 0)
    {
      usedTiles = new int[tiles.Length];
      for (int i = 0; i < usedTiles.Length; i++)
      {
        usedTiles[i] = i;
      }
    }

    if(tilesWeights.Length == 0)
    {
      tilesWeights = new int[tiles.Length];
      for (int i = 0; i < tilesWeights.Length; i++) tilesWeights[i] = 1;
    }

    grid = new Cell[gridWidth * gridHeight];
    for (int i = 0; i < grid.Length; i++)
    {
      grid[i] = new Cell();
      int cellY = Mathf.FloorToInt((float)i / gridWidth);
      int cellX = i - cellY * gridWidth;
      grid[i].name = cellX+"-"+cellY;
      grid[i].x = cellX;
      grid[i].y = cellY;
      for (int n = 0; n < usedTiles.Length; n++)
      {
        grid[i].candidates.Add(usedTiles[n]);
      }
    }

    // bind neighbours
    for (int i = 0; i < grid.Length; i++)
    {
      grid[i].left  = grid[i].x > 0 ? grid[  grid[i].x-1 + grid[i].y * gridWidth   ] : null;
      grid[i].right = grid[i].x < gridWidth-1 ? grid[  grid[i].x+1 + grid[i].y * gridWidth   ] : null;
      grid[i].up    = grid[i].y < gridHeight-1 ? grid[  grid[i].x + (grid[i].y+1) * gridWidth   ] : null;
      grid[i].down  = grid[i].y > 0 ? grid[  grid[i].x + (grid[i].y-1) * gridWidth   ] : null;
    }
  }

  IEnumerator RunAlgorithm()
  {
    bool finished = false;
    while(!finished)
    {
      // get a candidate
      int nextGridCell = FindLowestEntropyCell();
      if(nextGridCell == -1)
      {
        Debug.Log("--- algorithm completed");
        finished = true;
        break;
      }

      int cellY = Mathf.FloorToInt((float)nextGridCell / gridWidth);
      int cellX = nextGridCell - cellY * gridWidth;

      #if STEP_BY_STEP 
      yield return new WaitForInput(); 
      #endif


      // collapse
      int candidate = grid[nextGridCell].candidates[0];
      int sumWeights = 0;
      int numCandidates = grid[nextGridCell].candidates.Count;
      for (int i = 0; i < numCandidates; i++)
      {
        sumWeights += tilesWeights[grid[nextGridCell].candidates[i]];
      }
      int rnd = Random.Range(0, sumWeights);
      for (int i = 0; i < numCandidates; i++)
      {
        int candidateWeight = tilesWeights[grid[nextGridCell].candidates[i]];
        if(candidateWeight < rnd)
        {
          candidate =  grid[nextGridCell].candidates[i];
        }else
        {
          rnd -= candidateWeight;
        }
      }
      
      // update display
      for (int i = 0; i < grid[nextGridCell].candidates.Count; i++)
      {
        int c = grid[nextGridCell].candidates[i];
        if(c != candidate) RemoveFromCellDisplay(grid[nextGridCell], c);
      }
      ResolveCellDisplay(grid[nextGridCell], candidate);

      grid[nextGridCell].candidates.Clear();
      grid[nextGridCell].candidates.Add(candidate);

      // Debug.Log("collapsed cell to " + candidate);
      #if STEP_BY_STEP 
      yield return new WaitForInput(); 
      #endif


      // propagate wave

      yield return PropagateWave(new Vector2Int(cellX, cellY));

      #if STEP_BY_STEP 
      yield return new WaitForInput(); 
      #endif
    }
  }

  List<Vector2Int> exploredCells = new List<Vector2Int>();
  List<Vector2Int> explorationList = new List<Vector2Int>();
  IEnumerator PropagateWave(Vector2Int cellPos)
  {
    explorationList.Clear();
    exploredCells.Clear();

    explorationList.Add(cellPos);

    while(explorationList.Count > 0)
    {
      Vector2Int cellToExplore = explorationList[0];
      Cell currentCell = grid[cellToExplore.x + cellToExplore.y*gridWidth];
      if(!exploredCells.Contains(cellToExplore))
      {
        int numCandidatesLeft   = currentCell.left  != null ? currentCell.left.candidates.Count : 0;
        int numCandidatesRight  = currentCell.right != null ? currentCell.right.candidates.Count : 0;
        int numCandidatesUp     = currentCell.up    != null ? currentCell.up.candidates.Count : 0;
        int numCandidatesDown   = currentCell.down  != null ? currentCell.down.candidates.Count : 0;
        
        yield return PropagateCellChangesToNeighbours(currentCell.x, currentCell.y);
        exploredCells.Add(cellToExplore);

        int newCandidatesLeft   = currentCell.left  != null ? currentCell.left.candidates.Count : 0;
        int newCandidatesRight  = currentCell.right != null ? currentCell.right.candidates.Count : 0;
        int newCandidatesUp     = currentCell.up    != null ? currentCell.up.candidates.Count : 0;
        int newCandidatesDown   = currentCell.down  != null ? currentCell.down.candidates.Count : 0;

        if(currentCell.right != null && newCandidatesRight != numCandidatesRight && currentCell.right.candidates.Count > 1) explorationList.Add(cellToExplore + Vector2Int.right);

        if(currentCell.left != null && newCandidatesLeft != numCandidatesLeft && currentCell.left.candidates.Count > 1) explorationList.Add(cellToExplore - Vector2Int.right);

        if(currentCell.up != null && newCandidatesUp != numCandidatesUp && currentCell.up.candidates.Count > 1) explorationList.Add(cellToExplore + Vector2Int.up);

        if(currentCell.down != null && newCandidatesDown != numCandidatesDown && currentCell.down.candidates.Count > 1) explorationList.Add(cellToExplore - Vector2Int.up);
      }
      explorationList.RemoveAt(0);
    }

    yield return null;
  }

  IEnumerator PropagateCellChangesToNeighbours(int cellX, int cellY)
  {
    if(cellX < 0 || cellY < 0 || cellX >= gridWidth || cellY >= gridHeight) yield break;

    int cellIdx = cellX + cellY * gridWidth;
    if(cellX < gridWidth-1) yield return UpdateCellCandidates(grid[cellX + 1 + cellY*gridWidth ], grid[cellIdx], Direction.Right);
    if(cellY > 0) yield return  UpdateCellCandidates(grid[cellX + (cellY-1)*gridWidth ], grid[cellIdx], Direction.Down);
    if(cellX > 0) yield return UpdateCellCandidates(grid[cellX-1 + cellY*gridWidth], grid[cellIdx], Direction.Left);
    if(cellY < gridHeight-1) yield return UpdateCellCandidates(grid[cellX + (cellY+1)*gridWidth], grid[cellIdx], Direction.Up);

    yield return null;
  }

  // by taking a neighbour and direction, reevaluate this cell candidates
  // by checking if a rule exist linking the candidates to the neighbour
  IEnumerator UpdateCellCandidates(Cell cell, Cell neighbour, Direction dir)
  {
    // Debug.Log("Updating cell " + cell.name + " from neighbour " + neighbour.name + " with direction " + dir.ToString());

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

    if(cell.candidates.Count == 1)
    {
      ResolveCellDisplay(cell, cell.candidates[0]);
    }

    yield return null;
  }

  bool IsValidConstraint(int tilea, int tileb, Direction dir)
  {
    if(!rulesByStartID.ContainsKey(tilea)) return false;
    List<Constraint> tileaRules = rulesByStartID[tilea];
    for (int i = 0; i < tileaRules.Count; i++)
    {
      if( tileaRules[i].tile0 == tilea && 
          tileaRules[i].tile1 == tileb &&
          tileaRules[i].direction == dir)
      {
        return true;
      }
    }
    return false;
  }


  int FindLowestEntropyCell()
  {
    List<int> candidates = new List<int>();
    float lowestEntropy = Mathf.Infinity;

    for (int i = 0; i < grid.Length; i++)
    {
      if(grid[i].candidates.Count <= 1) continue;

      float entropy = grid[i].candidates.Count;
      if(entropy <= lowestEntropy)
      {
        lowestEntropy = entropy;
        candidates.Add(i);
      }
    }

    if(candidates.Count == 0) return -1;
    else return candidates[ Random.Range(0, candidates.Count) ];
  }

  GameObject gridDisplay;
  Dictionary<string, GameObject> gridDisplayObjects = new Dictionary<string, GameObject>();
  float zoomedTileScale; // scale value for a minified scale to reach full cell size (happens when only one tile is left in a cell)

  void ClearDisplay()
  {
    Destroy(gridDisplay);
    gridDisplayObjects.Clear();
  }

  void CreateGridDisplay()
  {
    float tileW = tiles[0].bounds.size.x;
    float tileH = tiles[0].bounds.size.y;
    float scale = 1f;

    int numTilesInCellW = Mathf.CeilToInt(Mathf.Sqrt(usedTiles.Length));
    int numTilesInCellH = numTilesInCellW;
    zoomedTileScale = numTilesInCellW;
    float cellW = tileW * scale * numTilesInCellW;
    float cellH = tileH * scale * numTilesInCellH;

    Vector3 startCorner = new Vector3();
    startCorner.x = -(cellW * gridWidth) * 0.5f;
    startCorner.y = -(cellH * gridHeight) * 0.5f;

    gridDisplay = new GameObject("GridDisplay");

    for (int y = 0; y < gridHeight; y++)
    {
      for (int x = 0; x < gridWidth; x++)
      {
        string name = x + "-" + y;
        GameObject root = new GameObject(name);
        root.transform.parent = gridDisplay.transform;
        root.transform.position = startCorner + Vector3.right * x * cellW + Vector3.up * y * cellH;
        gridDisplayObjects.Add(name, root);

        GameObject center = new GameObject("center");
        center.transform.parent = root.transform;
        center.transform.localPosition = Vector3.right * (cellW * 0.5f - tileW * scale * 0.5f) + Vector3.up * (cellH * 0.5f - tileH * scale * 0.5f);

        for (int i = 0; i < usedTiles.Length; i++)
        {
          int tileId = usedTiles[i];
          GameObject go = new GameObject();
          go.name = tileId.ToString();
          SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
          spr.sprite = tiles[ grid[y*gridWidth + x].candidates[i] ];
          go.transform.parent = root.transform;
          go.transform.localScale = Vector3.one * scale;

          float localX = (i%numTilesInCellW) * tileW * scale;
          float localY = Mathf.FloorToInt((float)i/numTilesInCellW) * tileH * scale;
          go.transform.localPosition = Vector3.right * localX + Vector3.up * localY;
        }
      }
    }

    // zoom camera to fit grid height
    // camera height = cellH * gridHeight
    Camera.main.orthographicSize = cellH * gridHeight * 0.5f;
  }

  void RemoveFromCellDisplay(Cell cell, int tileid)
  {
    for (int i = 0; i < grid.Length; i++)
    {
      if(grid[i] == cell)
      {
        int x = i % gridWidth;
        int y = Mathf.FloorToInt( (float)i/gridWidth );
        GameObject o = gridDisplayObjects[cell.name];
        Transform tile = o.transform.Find(""+tileid);
        if(tile != null)
        {
          tile.gameObject.SetActive(false);
        }else
        {
          Debug.LogErrorFormat("couldn't find tile {0} in cell {1}", tileid, cell.name);
        }
      }
    }
  }

  void ResolveCellDisplay(Cell cell, int tileid)
  {
    GameObject o = gridDisplayObjects[cell.name];
    Transform center = o.transform.Find("center");
    Transform tileTr = o.transform.Find(""+tileid);
    if(tileTr != null)
    {
      tileTr.localScale = new Vector3(zoomedTileScale, zoomedTileScale, 1);
      tileTr.position = center.position;
    }else
    {
      Debug.LogErrorFormat("couldn't find tile {0} in cell {1}", tileid, cell.name);
    }
  }


}
