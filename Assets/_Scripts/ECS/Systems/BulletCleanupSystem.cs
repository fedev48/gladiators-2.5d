using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct BulletCleanupSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI
            .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((RefRO<BulletDestroyTag> _, Entity entity) in
            SystemAPI.Query<RefRO<BulletDestroyTag>>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
        }
    }
}
