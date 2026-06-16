using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public partial class InputReaderSystem : SystemBase
{
    private InputSystem_Actions inputSystem;
    private static readonly quaternion CameraRot = quaternion.RotateY(math.radians(-135f));

    protected override void OnCreate()
    {
        inputSystem = new InputSystem_Actions();
        inputSystem.Enable();
    }

    protected override void OnUpdate()
    {
        var raw = inputSystem.Player.Move.ReadValue<Vector2>();
        float3 cameraFixedDirection = math.mul(CameraRot, new float3(raw.x, 0, raw.y));

        foreach (var moveDirection in SystemAPI.Query<RefRW<CharacterMoveDirection>>())
        {
            moveDirection.ValueRW.value = new float3(cameraFixedDirection.x, 0, cameraFixedDirection.z);
        }
    }

    protected override void OnStopRunning() => inputSystem.Disable();
}