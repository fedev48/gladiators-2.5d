using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public partial class InputReaderSystem : SystemBase
{
    private InputSystem_Actions inputSystem;
    private quaternion cameraRot;

    protected override void OnCreate()
    {
        inputSystem = new InputSystem_Actions();
        inputSystem.Enable();
    }

    protected override void OnStartRunning()
    {
        cameraRot = quaternion.RotateY(Camera.main.transform.eulerAngles.y * math.TORADIANS);
    }

    protected override void OnUpdate()
    {
        var raw = inputSystem.Player.Move.ReadValue<Vector2>();
        float3 cameraFixedDirection = math.mul(cameraRot, new float3(raw.x, 0, raw.y));

        foreach (var moveDirection in SystemAPI.Query<RefRW<CharacterMoveDirection>>())
        {
            moveDirection.ValueRW.value = new float3(cameraFixedDirection.x, 0, cameraFixedDirection.z);
        }

        if (inputSystem.Player.Interact.WasPressedThisFrame())
        {
            foreach ((RefRO<SkeletonSpellConfig> spellConfig, Entity entity) in
                SystemAPI.Query<RefRO<SkeletonSpellConfig>>().WithEntityAccess())
            {
                EntityManager.SetComponentData(entity, new SkeletonSpawnBurst
                {
                    remaining = spellConfig.ValueRO.spawnCount,
                    timer     = 0f
                });
                EntityManager.SetComponentEnabled<SkeletonSpawnBurst>(entity, true);
            }
        }
    }

    protected override void OnStopRunning() => inputSystem.Disable();
}