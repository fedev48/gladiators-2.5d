using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
[BurstCompile]
public partial struct MoveCharacterLocalSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (moveDirection, speed, transform) in SystemAPI
            .Query<RefRO<CharacterMoveDirection>, RefRO<CharacterMoveSpeed>, RefRW<LocalTransform>>()
            .WithAll<Player>())
        {
            float3 direction = moveDirection.ValueRO.value;
            float3 newPosition = transform.ValueRO.Position + direction * (speed.ValueRO.value * deltaTime);
            transform.ValueRW.Position = newPosition;
        }
    }
}