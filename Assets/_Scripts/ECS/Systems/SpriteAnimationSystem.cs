using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct SpriteAnimationSystem : ISystem
{
    [BurstCompile]
    public readonly void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (stateRef, clips, frames, uvRef) in
            SystemAPI.Query<RefRW<SpriteAnimationState>,
                           DynamicBuffer<AnimationClipData>,
                           DynamicBuffer<SpriteFrameElement>,
                           RefRW<SpriteUVRect>>())
        {
            ref var entityAnimationState = ref stateRef.ValueRW;

            int anim = math.clamp(entityAnimationState.currentAnimation, 0, clips.Length - 1);
            if (anim != entityAnimationState.prevAnimation)
            {
                entityAnimationState.currentAnimation = anim;
                entityAnimationState.currentFrame     = 0;
                entityAnimationState.elapsed          = 0f;
                entityAnimationState.prevAnimation    = anim;
            }

            var clip = clips[entityAnimationState.currentAnimation];
            if (clip.frameCount == 0) continue;

            entityAnimationState.elapsed += deltaTime;
            if (entityAnimationState.elapsed < 1f / clip.fps) continue;

            entityAnimationState.elapsed      -= 1f / clip.fps;
            entityAnimationState.currentFrame  = (entityAnimationState.currentFrame + 1) % clip.frameCount;
            uvRef.ValueRW.value = frames[clip.startIndex + entityAnimationState.currentFrame].uv;
        }
    }
}
