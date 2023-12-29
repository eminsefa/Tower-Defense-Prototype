#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using _Tower_Defense_Prototype.Game.Levels.Scripts;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Tower_Defense_Prototype.Game.Scripts.Managers
{
    //This helps converts data to path. Editor script for level creating
    //It uses tilemap but it could use persistent json file or cloud control to set data in runtime
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private LevelData m_LevelData;
        [SerializeField] private Tilemap   m_PathTileMap;

        [Button]
        private void ConvertTilemapToData()
        {
            //Get path tiles
            Transform grid       = m_PathTileMap.transform.parent;
            Transform gridParent = grid.parent;
            grid.SetParent(transform);
            
            BoundsInt                     bounds    = m_PathTileMap.cellBounds;
            Dictionary<Vector2, TileBase> pathTiles = new Dictionary<Vector2, TileBase>();

            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int x = 0; x < bounds.size.x; x++)
                {
                    Vector2Int cellLocalPosition = new Vector2Int(bounds.x + x, bounds.y + y);
                    TileBase   tile              = m_PathTileMap.GetTile((Vector3Int) cellLocalPosition);
                    var        isEmpty           = tile == null;
                    if (!isEmpty)
                    {
                        Vector2 worldCellPos = m_PathTileMap.GetCellCenterWorld((Vector3Int) cellLocalPosition);
                        pathTiles.Add(worldCellPos, tile);
                    }
                }
            }
            grid.SetParent(gridParent);
            
            
            //Create path by distance, starting from start
            Vector2[] pathPoints = new Vector2[pathTiles.Count];

            foreach (var positionTilePair in pathTiles)
            {
                if (positionTilePair.Value.name.Contains("Start"))
                {
                    pathPoints[0] = positionTilePair.Key;
                }
                else if (positionTilePair.Value.name.Contains("End"))
                {
                    pathPoints[^1] = positionTilePair.Key;
                }
            }

            for (var i = 1; i < pathPoints.Length - 1; i++)
            {
                var     minDist = float.MaxValue;
                Vector2 closest = Vector2.zero;
                foreach (var keyValuePair in pathTiles)
                {
                    if (pathPoints.Contains(keyValuePair.Key)) continue;

                    var prevPos  = pathPoints[i - 1];
                    var prevTile = pathTiles[prevPos];
                    if (i > 2)
                    {
                        var prevDir = pathPoints[i - 1] - pathPoints[i - 2];
                        
                        //To find correct path points, we need to add a logic, because its grid shaped
                        //This adds a bit offset to be able find correct route. Simple logic is enough for this case 
                        prevPos += prevDir * 0.1f * (prevTile.name.Contains("Rotate") ? -1f : 1);
                    }

                    var dist = Vector2.Distance(keyValuePair.Key, prevPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = keyValuePair.Key;
                    }
                }

                pathPoints[i] = closest;
            }

            
            //Create and save data
            m_LevelData.PathData             = new LevelData.LevelPathData
                                               {
                                                   GridSize = m_PathTileMap.cellSize.x,
                                                   Positions   = new Vector2[pathPoints.Length],
                                                   SplinePoints = new SplinePoint[pathPoints.Length]
                                               };

            for (var i = 0; i < pathPoints.Length; i++)
            {
                SplinePoint splinePoint = new SplinePoint(pathPoints[i]);
                m_LevelData.PathData.Positions[i]   = pathPoints[i];
                m_LevelData.PathData.SplinePoints[i] = splinePoint;
            }

            UnityEditor.EditorUtility.SetDirty(m_LevelData);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}

#endif