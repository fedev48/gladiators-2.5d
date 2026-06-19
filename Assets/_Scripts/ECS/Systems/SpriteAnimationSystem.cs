using Unity.Entities;
using UnityEngine;

public partial class SpriteAnimationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (stateRef, clips, frames, entity) in
            SystemAPI.Query<RefRW<SpriteAnimationState>,
                           DynamicBuffer<AnimationClipData>,
                           DynamicBuffer<SpriteFrameElement>>()
                     .WithEntityAccess())
        {
            ref var state = ref stateRef.ValueRW;

            var authoring = EntityManager.GetComponentObject<SpriteAnimatorAuthoring>(entity);
            state.currentAnimation = Mathf.Clamp(authoring.currentAnimation, 0, clips.Length - 1);

            // Reset frame when animation changes
            if (state.currentAnimation != state.prevAnimation)
            {
                state.currentFrame  = 0;
                state.elapsed       = 0f;
                state.prevAnimation = state.currentAnimation;
            }

            var clip = clips[state.currentAnimation];
            if (clip.frameCount == 0) continue;

            state.elapsed += dt;
            float frameDuration = 1f / clip.fps;

            if (state.elapsed < frameDuration) continue;

            state.elapsed     -= frameDuration;
            state.currentFrame = (state.currentFrame + 1) % clip.frameCount;

            var sr = EntityManager.GetComponentObject<SpriteRenderer>(entity);
            if (sr != null)
                sr.sprite = frames[clip.startIndex + state.currentFrame].sprite.Value;
        }
    }
}
