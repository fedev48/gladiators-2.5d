using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MoveCharacterLocalSystem))]
public partial struct GroundSnapSystem : ISystem
{
    private int groundMask;

    public void OnCreate(ref SystemState state)
    {
        groundMask = LayerMask.GetMask("Ground");
    }

    public void OnUpdate(ref SystemState state)
    {

        foreach (var (transform, _) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ShouldSnapToFloorTag>>())
        {
            float3 origin = transform.ValueRO.Position + new float3(0, 5f, 0);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 50f, groundMask))
            {
                var pos = transform.ValueRO.Position;
                pos.y = hit.point.y;
                transform.ValueRW.Position = pos;
            }
        }
    }
}
