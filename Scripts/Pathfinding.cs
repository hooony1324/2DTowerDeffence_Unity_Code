using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;


public class Pathfinding : MonoBehaviour
{
    private Grid grid;

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;


    public int[] FindPath(int2 spawnPoint, int2[] goalPositions)
    {
        grid = GameManager.Instance.GetGrid();
        int[] result = new int[999];

        for (int i = 0; i < goalPositions.Length; i++)
        {
            int[] path = GetAStarPath(spawnPoint, goalPositions[i]);

            if (path.Length < result.Length)
            {
                result = path;
            }
        }

        return result;
    }

    private int[] GetAStarPath(int2 spawnPoint, int2 goalPosition)
    {
        int gridWidth = grid.Width;
        int gridHeight = grid.Height;

        int[] gridArray = grid.GridArray;
        NativeArray<int> gridArrayN = new NativeArray<int>(gridArray.Length, Allocator.TempJob);
        gridArrayN.CopyFrom(gridArray);

        // Job Result
        NativeList<int> jobResult = new NativeList<int>(10, Allocator.TempJob);
        AStarSearch job = new AStarSearch()
        {
            gridWidth = gridWidth,
            gridHeight = gridHeight,
            startPosition = spawnPoint,
            endPosition = goalPosition,
            gridArray = gridArrayN,
            pathResult = jobResult
        };

        JobHandle handle = job.Schedule();
        handle.Complete();

        int[] path = new int[jobResult.Length];
        jobResult.ToArray().CopyTo(path, 0);

        jobResult.Dispose();
        gridArrayN.Dispose();

        return path;
    }


    [BurstCompile]
    public struct AStarSearch : IJob
    {
        public int gridWidth;
        public int gridHeight;
        public int2 startPosition;
        public int2 endPosition;
        public NativeArray<int> gridArray;
        public NativeList<int> pathResult;

        public void Execute()
        {
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridWidth * gridHeight, Allocator.Temp);

            // 정보 초기화
            for (int x = 0; x < gridWidth; ++x)
            {
                for (int y = 0; y < gridHeight; ++y)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.x = x;
                    pathNode.y = y;
                    pathNode.index = CalculateIndex(x, y, gridWidth);

                    pathNode.gCost = int.MaxValue;
                    pathNode.hCost = CalculateDistanceCost(new int2(x, y), endPosition);
                    pathNode.CalculateFCost();

                    bool isGridWalkable = gridArray[x + y * gridWidth] == 0;

                    pathNode.isWalkable = isGridWalkable;
                    pathNode.cameFromNodeIndex = -1;

                    pathNodeArray[pathNode.index] = pathNode;
                }
            }

            NativeArray<int2> neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighbourOffsetArray[0] = new int2(-1, 0); // Left
            neighbourOffsetArray[1] = new int2(+1, 0); // Right
            neighbourOffsetArray[2] = new int2(0, +1); // Up
            neighbourOffsetArray[3] = new int2(0, -1); // Down
            neighbourOffsetArray[4] = new int2(-1, -1); // Left Down
            neighbourOffsetArray[5] = new int2(-1, +1); // Left Up
            neighbourOffsetArray[6] = new int2(+1, -1); // Right Down
            neighbourOffsetArray[7] = new int2(+1, +1); // Right Up

            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridWidth);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridWidth)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;

            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
            
            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNodeIndex == endNodeIndex)
                {
                    // Reached destination
                    break;
                }

                // Remove current node from Open List
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                    }
                }

                closedList.Add(currentNodeIndex);

                // Find next valuable node
                for (int i = 0; i < neighbourOffsetArray.Length; i++)
                {
                    int2 neighbourOffset = neighbourOffsetArray[i];
                    int2 neighbourPosition = new int2(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y);

                    if (!IsPositionInsideGrid(neighbourPosition, new int2(gridWidth, gridHeight)))
                    {
                        // Skip invalid node
                        continue;
                    }

                    int neighbourNodeIndex = CalculateIndex(neighbourPosition.x, neighbourPosition.y, gridWidth);

                    if (closedList.Contains(neighbourNodeIndex))
                    {
                        // Skip founded node
                        continue;
                    }

                    PathNode neighbourNode = pathNodeArray[neighbourNodeIndex];
                    if (!neighbourNode.isWalkable)
                    {
                        continue;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    // find valuable neighbour
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePosition, neighbourPosition);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNodeIndex = currentNodeIndex;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.CalculateFCost();
                        pathNodeArray[neighbourNodeIndex] = neighbourNode;

                        if (!openList.Contains(neighbourNode.index))
                        {
                            openList.Add(neighbourNode.index);
                        }
                    }
                }
            }

            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // didn't find a path
                Debug.Log("didn't find a path");

            }
            else
            {
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
                for (int i = 0; i < path.Length; i++)
                {
                    pathResult.Add(path[i].x + path[i].y * gridWidth);
                }

                path.Dispose();
            }

            pathNodeArray.Dispose();
            neighbourOffsetArray.Dispose();
            openList.Dispose();
            closedList.Dispose();

        }

        private int CalculateIndex(int x, int y, int gridWidth)
        {
            return x + y * gridWidth;
        }

        private int CalculateDistanceCost(int2 aPosition, int2 bPosition)
        {
            int xDistance = math.abs(aPosition.x - bPosition.x);
            int yDistance = math.abs(aPosition.y - bPosition.y);
            int remaining = math.abs(xDistance - yDistance);
            // 둘 중에 짧은 변의 길이만큼 대각선 이동 가능
            // 나머지는 직각이동
            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        private int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
        {
            PathNode lowestCostPathNode = pathNodeArray[openList[0]];
            for (int i = 0; i < openList.Length; i++)
            {
                PathNode testPathNode = pathNodeArray[openList[i]];
                if (testPathNode.fCost < lowestCostPathNode.fCost)
                {
                    lowestCostPathNode = testPathNode;
                }
            }

            return lowestCostPathNode.index;
        }

        private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
        {
            return
                gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < gridSize.x &&
                gridPosition.y < gridSize.y;
        }

        private NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
        {
            if (endNode.cameFromNodeIndex == -1)
            {
                // couldn't find a path
                return new NativeList<int2>(Allocator.Temp);
            }
            else
            {
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                // endnode부터 startnode까지 거슬러서 path반환
                PathNode currentNode = endNode;
                while (currentNode.cameFromNodeIndex != -1)
                {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        private struct PathNode
        {
            public int x;
            public int y;

            public int index;

            public int gCost;           // 출발지부터 현재 노드 까지의 거리
            public int hCost;           // 현재 노드부터 목적지까지의 거리
            public int fCost;           // f = g + h

            public bool isWalkable;

            public int cameFromNodeIndex;

            public void CalculateFCost()
            {
                fCost = gCost + hCost;
            }

            public void SetIsWalkable(bool isWalkable)
            {
                this.isWalkable = isWalkable;
            }

        }
    }
}








