using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateRoom : MonoBehaviour
{
    private Tilemap ground, walls;
    public Tile groundTile, wallTile, doorTile, doorSideTile, wallEndTile;
    public Vector2Int size;

    // Top, Bottom, Left, Right
    public List<bool> doorSides;
    public List<bool> wallSides;
    public List<GenerateRoom> neighbours;

    public RoomType roomType;

    void Start()
    {

        ground = transform.Find("Ground").GetComponent<Tilemap>();
        walls = transform.Find("Walls").GetComponent<Tilemap>();

        // Simplifies coordinate arguments.
        size.Set(size.x - 1, size.y - 1);

        // Ensure wall corners are painted irregardless of wall sides.
        walls.SetTile(new Vector3Int(0, 0, 0), wallTile); // BR
        
        walls.SetTile(new Vector3Int(-size.x, 0, 0), wallTile); // BL
        
        
        for (int i = 0; i < size.x + 1; i++)
        {
            if (wallSides[0]) walls.SetTile(new Vector3Int(-i, size.y, 0), wallEndTile); // Top
            if (wallSides[1]) walls.SetTile(new Vector3Int(-i, 0, 0), wallTile); // Bottom

            for (int j = 0; j < size.y + 1; j++)
            {
                if (wallSides[2]) walls.SetTile(new Vector3Int(-size.x, j, 0), wallTile); // Left
                if (wallSides[3]) walls.SetTile(new Vector3Int(0, j, 0), wallTile); // Right

                ground.SetTile(new Vector3Int(-i, j, 0), groundTile);
            }
        }

        if (doorSides[0]) // Top
        {
            if (wallSides[0]) walls.SetTile(new Vector3Int(-size.x / 2, size.y, 0), doorTile);
        }
        if (doorSides[1]) // Bottom
        {
            if (wallSides[1]) walls.SetTile(new Vector3Int(-size.x / 2, 0, 0), doorTile);
        }
        if (doorSides[2]) // Left
        {
            if (wallSides[2])
            {
                walls.SetTile(new Vector3Int(-size.x, size.y / 2, 0), doorSideTile);
                walls.SetTile(new Vector3Int(-size.x, size.y / 2 + 1, 0), wallEndTile);
            } else
            {
                walls.SetTile(new Vector3Int(-size.x, size.y, 0), neighbours[2].wallSides[0] ? wallTile : wallEndTile); //TL
            }
        }
        if (doorSides[3]) // Right
        {
            if (wallSides[3])
            {
                walls.SetTile(new Vector3Int(0, size.y / 2, 0), doorSideTile);
                walls.SetTile(new Vector3Int(0, size.y / 2 + 1, 0), wallEndTile);
            } else
            {
                walls.SetTile(new Vector3Int(0, size.y, 0), neighbours[3].wallSides[0] ? wallTile : wallEndTile); // TR
            }
        } 

    
    }
}

public enum RoomType
{
    Start,
    Base,
    Boss,
    Key,
    Shop
}
