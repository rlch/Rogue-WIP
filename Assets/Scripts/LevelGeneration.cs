using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TODO: optimise neighbour finding by creating a dictionary
/// </summary>

public class LevelGeneration : MonoBehaviour
{
    public GameObject roomPrefab;
    public Vector2Int gridSize;
    // change this
    public Vector2Int roomSize;

    private List<Vector2Int> takenPositions = new List<Vector2Int>();
    private Dictionary<Vector2Int, GenerateRoom> roomGens;

    void Start()
    {
        takenPositions.Add(Vector2Int.zero);

        Vector2Int nextPosition;
        // Probability that rooms of 1 neighbour will be spawned.
        float spawnProb, spawnProbLower = 0.875f, spawnProbUpper = 0.995f;

        for (int i = 0; i < gridSize.x * gridSize.y - 1; i++)
        {
            Debug.Log($"Spawning Room {i.ToString()}");
            // approaches [spawnProbUpper] as i -> total size of grid.
            spawnProb = Mathf.Lerp(spawnProbLower, spawnProbUpper, i / (gridSize.x * gridSize.y));
            nextPosition = GetValidPosition();

            if (GetAmountOfNeighbours(nextPosition) > 1 && UnityEngine.Random.value < spawnProb)
            {
                nextPosition = GetValidPosition(findSingleNeighbour: true);
                Debug.Log($"Found position {i.ToString()}");
            }

            takenPositions.Add(nextPosition);

        }


        roomGens = new Dictionary<Vector2Int, GenerateRoom>();
        takenPositions.ForEach((pos) =>
        {
            Debug.Log($"Instantiating {pos.ToString()}");
            List<bool> doorSides = new List<bool> {
                takenPositions.Contains(new Vector2Int(pos.x, pos.y + 1)),
                takenPositions.Contains(new Vector2Int(pos.x, pos.y - 1)),
                takenPositions.Contains(new Vector2Int(pos.x - 1, pos.y)),
                takenPositions.Contains(new Vector2Int(pos.x + 1, pos.y))
            };

            GameObject room = Instantiate(roomPrefab, new Vector2(pos.x * roomSize.x, pos.y * roomSize.y), Quaternion.identity);
            GenerateRoom roomGen = room.GetComponent<GenerateRoom>();

            roomGen.doorSides = doorSides;
            // Initialize wall sides for next step
            roomGen.wallSides = new List<bool>() { true, true, true, true };
            roomGens[pos] = roomGen;
        });

        foreach (KeyValuePair<Vector2Int, GenerateRoom> kv in roomGens)
        {
            Vector2Int position = kv.Key;
            GenerateRoom roomGen = kv.Value;
            List<bool> prevWallSides = roomGen.wallSides;
            List<bool> doorSides = roomGen.doorSides;

            roomGen.neighbours = new List<GenerateRoom>() {
                    doorSides[0] ? roomGens[new Vector2Int(position.x, position.y + 1)] : null,
                    doorSides[1] ? roomGens[new Vector2Int(position.x, position.y - 1)] : null,
                    doorSides[2] ? roomGens[new Vector2Int(position.x - 1, position.y)] : null,
                    doorSides[3] ? roomGens[new Vector2Int(position.x + 1, position.y)] : null,
            };

            float wallSideProb = (float)(doorSides.Sum(Convert.ToDecimal) / 4);

            // If there is no door, we always want a wall.
            List<bool> newWallSides = new List<bool>
            {
                prevWallSides[0] && (!doorSides[0] || wallSideProb > UnityEngine.Random.value),
                prevWallSides[1] && (!doorSides[1] || wallSideProb > UnityEngine.Random.value),
                prevWallSides[2] && (!doorSides[2] || wallSideProb > UnityEngine.Random.value),
                prevWallSides[3] && (!doorSides[3] || wallSideProb > UnityEngine.Random.value),
            };

            roomGen.wallSides = newWallSides;
            if (!newWallSides[0]) // If no wall on top, remove bottom of y + 1
            {
                roomGens[new Vector2Int(position.x, position.y + 1)].wallSides[1] = false;
            }
            if (!newWallSides[1]) // Remove top of y - 1
            {
                roomGens[new Vector2Int(position.x, position.y - 1)].wallSides[0] = false;
            }
            if (!newWallSides[2]) // Remove right of x - 1
            {
                roomGens[new Vector2Int(position.x - 1, position.y)].wallSides[3] = false;
            }
            if (!newWallSides[3]) // Remove left of x + 1
            {
                roomGens[new Vector2Int(position.x + 1, position.y)].wallSides[2] = false;
            }
        }

    }

    private Vector2Int GetValidPosition(bool findSingleNeighbour = false)
    {
        System.Random random = new System.Random();
        Vector2Int position = Vector2Int.zero;
        do
        {
            Debug.Log($"Position invalid");

            if (findSingleNeighbour)
            {
                for (int i = 0; i < 100; i++)
                {
                    position = takenPositions[random.Next(takenPositions.Count)];
                    Debug.Log($"Finding single neighbour: index {i.ToString()}");
                    if (GetAmountOfNeighbours(position) == 1)
                    {
                        print("found");
                        break;
                    }
                }
            }
            else
            {
                position = takenPositions[random.Next(takenPositions.Count)];
            }


            // Randomly increments/decrements x or y.
            if (random.NextDouble() > 0.5)
            {
                position.x += 1 - (2 * Convert.ToInt32(random.NextDouble() > 0.5));
            }
            else
            {
                position.y += 1 - (2 * Convert.ToInt32(random.NextDouble() > 0.5));
            }

        } while (
            takenPositions.Contains(position) ||
            position.x >= gridSize.x ||
            position.x < -gridSize.x ||
            position.y >= gridSize.y ||
            position.y < -gridSize.y);
        return position;
    }

    int GetAmountOfNeighbours(Vector2Int position)
    {
        int amount = 0;
        if (takenPositions.Contains(position + Vector2Int.right))
        {
            amount++;
        }
        if (takenPositions.Contains(position + Vector2Int.left))
        {
            amount++;
        }
        if (takenPositions.Contains(position + Vector2Int.right))
        {
            amount++;
        }
        if (takenPositions.Contains(position + Vector2Int.right))
        {
            amount++;
        }
        return amount;
    }
}
