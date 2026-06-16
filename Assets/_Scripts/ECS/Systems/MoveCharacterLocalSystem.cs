using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
[BurstCompile]
public partial struct MoveCharacterLocalSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (moveDirection, speed, velocity) in SystemAPI
            .Query<RefRO<CharacterMoveDirection>, RefRO<CharacterMoveSpeed>, RefRW<PhysicsVelocity>>()
            .WithAll<PlayerTag>())
        {
            float3 direction = moveDirection.ValueRO.value;
            velocity.ValueRW.Linear = direction * speed.ValueRO.value;
            velocity.ValueRW.Angular = float3.zero;
        }
    }
}