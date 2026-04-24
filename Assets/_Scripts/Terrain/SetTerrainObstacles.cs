using UnityEngine;

public class SetTerrainObstacles : MonoBehaviour
{
    void Start()
    {
        Terrain terrain = Terrain.activeTerrain;

        float width  = terrain.terrainData.size.x;
        float height = terrain.terrainData.size.y;
        float length = terrain.terrainData.size.z;

        TreeInstance[] trees = terrain.terrainData.treeInstances;
        GameObject parent = new GameObject("Tree_Obstacles");
        parent.transform.position = terrain.GetPosition();

        int count = 0;

        foreach (TreeInstance tree in trees)
        {
            GameObject prefab = terrain.terrainData.treePrototypes[tree.prototypeIndex].prefab;

            if (prefab.GetComponent<Collider>() == null)
            {
                Debug.LogWarning($"'{prefab.name}' no tiene Collider, se omite.");
                continue;
            }

            Vector3 worldPos = new Vector3(
                tree.position.x * width,
                tree.position.y * height,
                tree.position.z * length
            ) + terrain.GetPosition();

            Quaternion rot = Quaternion.AngleAxis(tree.rotation * Mathf.Rad2Deg, Vector3.up);

            GameObject obs = Instantiate(prefab, worldPos, rot);
            obs.transform.SetParent(parent.transform);
            obs.name = "TreeObstacle_" + count;

            count++;
        }

        Debug.Log($"{count} tree colliders instanciados.");
    }
}