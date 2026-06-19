using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SpriteAnimatorAuthoring : MonoBehaviour
{
    public List<SpriteAnimationClip> animations;
    public int initialAnimation  = 0;
    public int currentAnimation  = 0;

    public class Baker : Baker<SpriteAnimatorAuthoring>
    {
        public override void Bake(SpriteAnimatorAuthoring authoring)
        {
            if (authoring.animations == null || authoring.animations.Count == 0) return;

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponentObject(entity, authoring);

            AddComponent(entity, new SpriteAnimationState
            {
                currentAnimation = authoring.initialAnimation,
                currentFrame     = 0,
                elapsed          = 0f,
                prevAnimation    = -1
            });

            var clipBuffer  = AddBuffer<AnimationClipData>(entity);
            var frameBuffer = AddBuffer<SpriteFrameElement>(entity);

            int frameOffset = 0;
            foreach (var clip in authoring.animations)
            {
                clipBuffer.Add(new AnimationClipData
                {
                    startIndex = frameOffset,
                    frameCount = clip.frames?.Count ?? 0,
                    fps        = Mathf.Max(clip.fps, 0.01f)
                });

                if (clip.frames != null)
                    foreach (var sprite in clip.frames)
                        frameBuffer.Add(new SpriteFrameElement { sprite = sprite });

                frameOffset += clip.frames?.Count ?? 0;
            }
        }
    }
}

[Serializable]
public class SpriteAnimationClip
{
    public string     name;
    public List<Sprite> frames;
    public float      fps = 8f;
}

public struct SpriteAnimationState : IComponentData
{
    public int   currentAnimation;
    public int   currentFrame;
    public float elapsed;
    public int   prevAnimation;
}

[InternalBufferCapacity(4)]
public struct AnimationClipData : IBufferElementData
{
    public int   startIndex;
    public int   frameCount;
    public float fps;
}

[InternalBufferCapacity(16)]
public struct SpriteFrameElement : IBufferElementData
{
    public UnityObjectRef<Sprite> sprite;
}
