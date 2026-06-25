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

        foreach (var (stateRef, clips, frames, uvRef, oneShotEnabled) in
            SystemAPI.Query<RefRW<SpriteAnimationState>,
                           DynamicBuffer<AnimationClipData>,
                           DynamicBuffer<SpriteFrameElement>,
                           RefRW<SpriteUVRect>,
                           EnabledRefRW<OneShotAnimTag>>()
                .WithPresent<OneShotAnimTag>())
        {
            ref var s = ref stateRef.ValueRW;

            int anim = s.currentAnimation;
            for (int guard = 0; guard < clips.Length; guard++)
            {
                if (anim < 0 || anim >= clips.Length || clips[anim].overrideTo < 0) break;
                anim = clips[anim].overrideTo;
            }
            anim = math.clamp(anim, 0, clips.Length - 1);

            s.currentAnimation = anim;
            if (anim != s.prevAnimation)
            {
                s.currentFrame  = 0;
                s.elapsed       = 0f;
                s.prevAnimation = anim;
            }

            var clip = clips[s.currentAnimation];
            if (clip.frameCount == 0) continue;

            s.elapsed += deltaTime;
            if (s.elapsed < 1f / clip.fps) continue;

            s.elapsed -= 1f / clip.fps;

            int nextFrame = s.currentFrame + 1;
            if (nextFrame >= clip.frameCount)
            {
                if (oneShotEnabled.ValueRO)
                {
                    oneShotEnabled.ValueRW = false; 
                    s.currentFrame = 0;
                }
                else
                {
                    s.currentFrame = 0; 
                }
            }
            else
            {
                s.currentFrame = nextFrame;
            }

            uvRef.ValueRW.value = frames[clip.startIndex + s.currentFrame].uv;
        }
    }
}
