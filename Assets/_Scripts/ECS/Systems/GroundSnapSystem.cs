using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MoveCharacterLocalSystem))]
public partial struct GroundSnapSystem : ISystem
{
    private int groundMask;
    private const float yOffset = 1f;

    public void OnCreate(ref SystemState state)
    {
        groundMask = LayerMask.GetMask("Ground");
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, _) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerTag>>())
        {
            float3 origin = transform.ValueRO.Position + new float3(0, yOffset, 0);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f, groundMask))
            {
                var pos = transform.ValueRO.Position;
                pos.y = hit.point.y + yOffset;
                transform.ValueRW.Position = pos;
            }
        }
    }
}
