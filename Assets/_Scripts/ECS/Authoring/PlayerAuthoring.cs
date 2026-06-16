using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity,new Player());
            AddComponent(entity, new CharacterMoveSpeed { value = 5 });
            AddComponent(entity, new CharacterMoveDirection {});
        }
    }
}

public struct Player : IComponentData
{
    
}
public struct CharacterMoveDirection : IComponentData
{
    public float3 value;
}

public struct CharacterMoveSpeed : IComponentData
{
    public float value;
}