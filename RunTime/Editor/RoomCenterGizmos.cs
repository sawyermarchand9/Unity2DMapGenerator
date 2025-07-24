using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace MapGeneration
{
    public class RoomCenterGizmos : MonoBehaviour
    {
        public Tilemap targetTilemap;
        public MapGeneration.RandomWalkGenerator generator;

        void OnDrawGizmos()
        {
            if (generator == null || generator.roomCenters == null)
                return;

            Gizmos.color = Color.red;
            foreach (var center in generator.roomCenters)
            {
                // Offset by tilemap position if needed
                Vector3 worldPos = new Vector3(center.x, center.y, 0);
                if (targetTilemap != null)
                    worldPos += targetTilemap.transform.position;
                Gizmos.DrawSphere(worldPos, 0.5f);
            }
        }
    }
}