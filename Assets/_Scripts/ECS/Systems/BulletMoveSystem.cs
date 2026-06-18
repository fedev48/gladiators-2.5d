using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public partial struct BulletMoveSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<PhysicsVelocity> velocity, RefRO<BulletConfig> config) in
            SystemAPI.Query<RefRW<PhysicsVelocity>, RefRO<BulletConfig>>())
        {
            velocity.ValueRW.Linear  = config.ValueRO.direction * config.ValueRO.speed;
            velocity.ValueRW.Angular = Unity.Mathematics.float3.zero;
        }
    }
}
