using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
public partial struct SkeletonFollowSystem : ISystem
{
    [BurstCompile]
    public readonly void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        float3 playerPos = float3.zero;
        foreach (RefRO<LocalTransform> playerTransform in
            SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerTag>())
        {
            playerPos = playerTransform.ValueRO.Position;
        }

        foreach ((RefRW<LocalTransform> transform,
                  RefRW<SkeletonSpawnData> spawnData,
                  RefRO<SkeletonConfig> config,
                  RefRW<PhysicsVelocity> velocity) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<SkeletonSpawnData>, RefRO<SkeletonConfig>, RefRW<PhysicsVelocity>>()
                .WithAll<SkeletonFollowState>())
        {
            if (spawnData.ValueRO.followOffset.Equals(float3.zero))
                spawnData.ValueRW.followOffset = transform.ValueRO.Position - playerPos;

            float3 target   = playerPos + spawnData.ValueRO.followOffset;
            float3 toTarget = target - transform.ValueRO.Position;
            toTarget.y = 0f;

            float  dist         = math.length(toTarget);
            float3 direction    = math.normalizesafe(toTarget);
            float  acceleration = config.ValueRO.acceleration;
            float  currentSpeed = spawnData.ValueRO.currentSpeed;

            // Stopping distance: v² / (2a)
            float stoppingDist = currentSpeed * currentSpeed / (2f * acceleration);

            if (dist > stoppingDist + 0.05f)
                currentSpeed = math.min(currentSpeed + acceleration * dt, config.ValueRO.maxSpeed);
            else
                currentSpeed = math.max(currentSpeed - acceleration * dt, 0f);

            spawnData.ValueRW.currentSpeed = currentSpeed;

            velocity.ValueRW.Linear  = new float3(direction.x * currentSpeed, 0f, direction.z * currentSpeed);
            velocity.ValueRW.Angular = float3.zero;
            transform.ValueRW.Rotation = quaternion.identity;
        }
    }
}
