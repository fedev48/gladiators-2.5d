using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InputReaderSystem))]
[BurstCompile]
public partial struct FireBulletSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences refs = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<LocalTransform> transform,
                  RefRO<FireBulletEvent> orbEvent,
                  Entity entity) in
            SystemAPI.Query<RefRO<LocalTransform>, RefRO<FireBulletEvent>>()
                .WithEntityAccess())
        {
            BulletConfig prefabConfig = SystemAPI.GetComponent<BulletConfig>(refs.bulletPrefabEntity);
            prefabConfig.direction = orbEvent.ValueRO.direction;

            float3 spawnPos = transform.ValueRO.Position + new float3(0f, 1f, 0f);

            Entity orb = ecb.Instantiate(refs.bulletPrefabEntity);
            ecb.SetComponent(orb, LocalTransform.FromPosition(spawnPos));
            ecb.SetComponent(orb, prefabConfig);

            SystemAPI.SetComponentEnabled<FireBulletEvent>(entity, false);
        }
    }
}
