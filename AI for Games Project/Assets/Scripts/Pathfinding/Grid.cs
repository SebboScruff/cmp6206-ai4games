using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayNodeWeights;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDict = new Dictionary<int, int>();

    PathfindNode[,] grid;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    [Range(1,10)]public int blurSize = 1; // alter the resolution of node weight blurring in inspector
    // penalty min/max variables for grid visualisation purposes
    public int penaltyMin{get; private set;}
    public int penaltyMax{get; private set;}

    [SerializeField] GameObject nodeVisualPrefab;
    Color visNodeColour;
    [SerializeField] float visualNodeHeight = 0.4f;

    // Generate the grid on Awake so it is available before any path requests
    void Awake()
    {
        // a reminder for future seb if he needs one
        if(displayNodeWeights){
            Debug.Log("Node View is best seen from Orthographic-Top Scene View!");
        }

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // Setup Walkable Terrain Types
        foreach(TerrainType region in walkableRegions){
            walkableMask.value |= region.terrainMask.value; // bitwise OR operator to add layers to the mask by numerical value
            walkableRegionsDict.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);            
        }

        penaltyMin = int.MaxValue;

        penaltyMax = int.MinValue;

        CreateGrid();
        CreateVisualGrid();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5)){
            displayNodeWeights = !displayNodeWeights;
        }
    }

    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    void CreateGrid()
    {
        grid = new PathfindNode[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - (Vector3.right * gridWorldSize.x / 2) - (Vector3.forward * gridWorldSize.y / 2);

        for(int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                int movementPenalty = 0;

                // Initial Node Setup - is there anything unwalkable in the immediate area?
                bool isNodeWalkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                // Second Step: Set Node Weights
                if(isNodeWalkable){
                    float rayStartHeight = 50f;
                    Ray ray = new Ray(worldPoint + Vector3.up * rayStartHeight, Vector3.down); // start at a point arbitrarily above the node
                    RaycastHit hit;
                    // raycast downwards, then check if the raycast hit is in the walkable mask:
                    // if it is, update the movement cost; if the ray doesn't hit anything, make that node unwalkable
                    if(Physics.Raycast(ray, out hit, 100)){
                        // Mark the nodes as unwalkable if they are A) too high up or 2) not in the walkable layer
                        if(!walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty)){
                            isNodeWalkable = false;
                        }
                        if(hit.transform.tag == "Agent" || hit.transform.tag == "Player"){
                            isNodeWalkable = true;
                        }

                        worldPoint.y = hit.point.y + 1f;

                        // Adapt the node's Movement Cost based on gameobject layers or terrain layers
                        if(hit.collider.TryGetComponent<Terrain>(out Terrain t_)){
                            movementPenalty = GetWeightFromTerrain(t_, hit.point);
                        }
                        else{
                            walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        }                        
                    }
                    else{
                        isNodeWalkable = false;
                    }
                }
                // Then declare the node into the array
                grid[x, y] = new PathfindNode(isNodeWalkable, worldPoint, x, y, movementPenalty);
            }
        }

        BlurPenaltyMap(blurSize);
    }

    // This function uses Box Blur to smoothly weight the nodes in the grid to allow for a generally more convincing path
    // Primary use-case: Making an agent have bias to the center of a path rather than the edge of the same path
    void BlurPenaltyMap(int _blurSize)
    {
        // Example: Putting blurSize = 1 will use a kernal grid of 1 node in each direction, i.e. a 3x3 grid
        int kernelSize = _blurSize*2 + 1;
        int kernelExtents = (kernelSize-1)/2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        // Doing the Box Blur algorithm in 2 passes is more efficient than checking a (n*n) kernel box once per node
        // Horizontal pass first
        for(int y = 0; y < gridSizeY; y++){
            for(int x = -kernelExtents; x <= kernelExtents; x++){
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }
            for(int x = 1; x < gridSizeX; x++){
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] += penaltiesHorizontalPass[x-1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }
        // Then the vertical pass
        for(int x = 0; x < gridSizeX; x++){
            for(int y = -kernelExtents; y <= kernelExtents; y++){
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesVerticalPass[x, sampleY];
            }
            for(int y = 1; y < gridSizeY; y++){
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);
                
                penaltiesVerticalPass[x, y] += penaltiesVerticalPass[x, y-1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];

                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x,y] / (kernelSize*kernelSize));
                grid[x,y].movementPenalty = blurredPenalty;

                // for node visualisation purposes
                if(blurredPenalty > penaltyMax){
                    penaltyMax = blurredPenalty;
                }
                if(blurredPenalty < penaltyMin){
                    penaltyMin = blurredPenalty;
                }
            }
        }
    }

    public List<PathfindNode> GetNeighbours(PathfindNode node)
    {
        List<PathfindNode> neighbours = new List<PathfindNode>();

        for(int x = -1; x <= 1; x++){
            for (int y = -1; y <= 1; y++){
                if(x == 0 && y == 0) { continue; }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public PathfindNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY= Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    // public List<PathfindNode> path;

    // void OnDrawGizmos(){
    //     Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); // visualise the boundaries of the a* grid

    //     if(grid != null && displayNodeWeights){
    //         foreach(PathfindNode n in grid)
    //         {
    //             // Greyscale the walkable tiles based on their weights, then color the unwalkable nodes red
    //             Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
    //             Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
    //             Gizmos.color = new Color(Gizmos.color.r,Gizmos.color.g,Gizmos.color.b,0.4f); // make the visual nodes slightly translucent
    //             // if (path != null)
    //             // {
    //             //     if (path.Contains(n)) { Gizmos.color = Color.black; }
    //             // }
    //             Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
    //         }
    //     }
    // }

    void CreateVisualGrid()
    {
        if(grid != null){
            foreach(PathfindNode n in grid){

                visNodeColour = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));

                visNodeColour = (n.walkable) ? visNodeColour : Color.red;

                visNodeColour = new Color(visNodeColour.r, visNodeColour.g, visNodeColour.b, 0.2f);

                // Instantiate a cube at the point
                GameObject visNode = Instantiate(nodeVisualPrefab, n.worldPosition, Quaternion.identity);
                // adjust color, size, etc.
                visNode.transform.parent = this.transform;
                visNode.transform.localScale *= nodeDiameter;
                visNode.transform.localScale = new Vector3(visNode.transform.localScale.x,
                                                            visualNodeHeight,
                                                            visNode.transform.localScale.z);

                visNode.GetComponent<MeshRenderer>().material.color = visNodeColour;
            }
        }
    }

    // this class is how different costs are assigned to different layers
    [System.Serializable] 
    public class TerrainType
    {
        public LayerMask terrainMask;
        public Texture albedo;
        public int terrainPenalty;   
    }
    
    private int GetWeightFromTerrain(Terrain _terrain, Vector3 _hitPoint)
    {
        int weight = 0;
        Vector3 terrainPos = _hitPoint - _terrain.transform.position; // where on the terrain is the Hit Point?
        Vector3 splatMapPos = new Vector3(terrainPos.x / _terrain.terrainData.size.x, 
                                        0,
                                        terrainPos.z / _terrain.terrainData.size.z);
        int x = Mathf.FloorToInt(splatMapPos.x * _terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPos.z * _terrain.terrainData.alphamapHeight);
        float[,,] alphaMap = _terrain.terrainData.GetAlphamaps(x,z,1,1); // get the alpha map at the hit position, with a 1x1 px size

        // find the most prevalent texture at this point
        int primaryIndex = 0;
        for(int i = 0; i < alphaMap.Length; i++){
            if(alphaMap[0,0,i] > alphaMap[0,0,primaryIndex]){
                primaryIndex = i;
            }
        }
        foreach(TerrainType region in walkableRegions){
            if(region.albedo == _terrain.terrainData.terrainLayers[primaryIndex].diffuseTexture){
                weight = region.terrainPenalty;
            }
        }

        return weight;
    }
}


