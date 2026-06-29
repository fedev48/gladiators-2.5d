using Unity.Entities;
using Unity.Mathematics;

public struct UnitTag : IComponentData {}

//movement
public struct MoveDirection : IComponentData { public float3 value; }
public struct MoveSpeed     : IComponentData { public float  value; }
public struct NeedsPathfinding : IComponentData, IEnableableComponent { public int flowFieldId; }


//animation
public struct VisualEntity      : IComponentData { public Entity value; }
public struct FacingDirection   : IComponentData { public float3 value; }
public struct UnitMovementAnimTag : IComponentData {}
public struct OneShotAnimTag : IComponentData, IEnableableComponent {}


