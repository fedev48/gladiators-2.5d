using Unity.Entities;
using UnityEngine;

public class PrefabsReferencesAuthoring : MonoBehaviour
{
    public GameObject skeletonPrefabGameObject;

    public class Baker : Baker<PrefabsReferencesAuthoring>
    {
        public override void Bake(PrefabsReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                skeletonPrefabEntity = GetEntity(authoring.skeletonPrefabGameObject, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct EntitiesReferences : IComponentData
{
    public Entity skeletonPrefabEntity;
}
