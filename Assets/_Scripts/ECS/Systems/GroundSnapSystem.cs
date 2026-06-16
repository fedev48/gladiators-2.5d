using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(MoveCharacterLocalSystem))]
public partial class GroundSnapSystem : SystemBase
{
    private LayerMask groundMask;
    private static readonly float yOffset = 1f;

    protected override void OnCreate()
    {
        groundMask = LayerMask.GetMask("Ground");
    }

    protected override void OnUpdate()
    {
        foreach (var (transform, _) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<Player>>())
        {
            float3 origin = transform.ValueRO.Position + new float3(0, 1f, 0);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f, groundMask))
            {
                var pos = transform.ValueRO.Position;
                pos.y = hit.point.y + yOffset;
                transform.ValueRW.Position = pos;
            }
        }
    }
}
