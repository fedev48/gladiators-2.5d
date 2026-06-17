using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField] float playerSpeed = 5f;
    [SerializeField] float skeletonSpawnMinRadius = 3f;
    [SerializeField] float skeletonSpawnMaxRadius = 8f;
    [SerializeField] int skeletonSpawnCount = 3;
    [SerializeField] float skeletonSpawnInterval = 0.3f;

    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new PlayerTag());
            AddComponent(entity, new CharacterMoveSpeed { value = authoring.playerSpeed });
            AddComponent(entity, new CharacterMoveDirection {});
            AddComponent(entity, new ShouldSnapToFloorTag());
            AddComponent(entity, new SummonSkeletonEvent());
            AddComponent(entity, new SkeletonSpellConfig
            {
                minRadius    = authoring.skeletonSpawnMinRadius,
                maxRadius    = authoring.skeletonSpawnMaxRadius,
                spawnCount   = authoring.skeletonSpawnCount,
                interval     = authoring.skeletonSpawnInterval
            });
            AddComponent(entity, new SkeletonSpawnBurst());
            SetComponentEnabled<SkeletonSpawnBurst>(entity, false);

            SetComponentEnabled<SummonSkeletonEvent>(entity, false);
        }
    }
}

public struct SkeletonSpellConfig : IComponentData
{
    public float minRadius;
    public float maxRadius;
    public int   spawnCount;
    public float interval;
}

public struct SkeletonSpawnBurst : IComponentData, IEnableableComponent
{
    public int   remaining;
    public float timer;
}

public struct PlayerTag : IComponentData {}
public struct ShouldSnapToFloorTag : IComponentData, IEnableableComponent {}
public struct SummonSkeletonEvent : IComponentData, IEnableableComponent {}
public struct CharacterMoveDirection : IComponentData
{
    public float3 value;
}

public struct CharacterMoveSpeed : IComponentData
{
    public float value;
}

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial struct PlayerFreezeRotationBakingSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var mass in SystemAPI.Query<RefRW<PhysicsMass>>().WithAll<PlayerTag>())
        {
            mass.ValueRW.InverseInertia = float3.zero;
        }
    }
}