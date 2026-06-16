using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    [SerializeField] float playerSpeed = 5f;
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity,new PlayerTag());
            AddComponent(entity, new CharacterMoveSpeed { value = authoring.playerSpeed });
            AddComponent(entity, new CharacterMoveDirection {});
        }
    }
}

public struct PlayerTag : IComponentData {}
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