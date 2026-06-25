using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class SpriteAnimatorAuthoring : MonoBehaviour
{
    public List<SpriteAnimationClip> animations;
    public int     initialAnimation = 0;
    public int     currentAnimation = 0;
    public Vector2 flipPivotOffset  = Vector2.zero;
    public float   cameraYAngle     = 0f;

    public class Baker : Baker<SpriteAnimatorAuthoring>
    {
        public override void Bake(SpriteAnimatorAuthoring authoring)
        {
            if (authoring.animations == null || authoring.animations.Count == 0) return;

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

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
                if (clip.isOverride && clip.flip)
                {
                    
                    var target     = authoring.animations[clip.overrideTo];
                    int frameCount = target.frames?.Count ?? 0;
                    clipBuffer.Add(new AnimationClipData
                    {
                        startIndex = frameOffset,
                        frameCount = frameCount,
                        fps        = Mathf.Max(target.fps, 0.01f),
                        overrideTo = -1
                    });
                    if (target.frames != null)
                    {
                        float2 po = (float2)authoring.flipPivotOffset;
                        foreach (var sprite in target.frames)
                        {
                            float4 uv = SpriteToUV(sprite);
                            frameBuffer.Add(new SpriteFrameElement { uv = new float4(uv.x + uv.z + po.x, uv.y + po.y, -uv.z, uv.w) });
                        }
                    }
                    frameOffset += frameCount;
                }
                else
                {
                    clipBuffer.Add(new AnimationClipData
                    {
                        startIndex = frameOffset,
                        frameCount = clip.isOverride ? 0 : (clip.frames?.Count ?? 0),
                        fps        = Mathf.Max(clip.fps, 0.01f),
                        overrideTo = clip.isOverride ? clip.overrideTo : -1
                    });
                    if (!clip.isOverride && clip.frames != null)
                    {
                        foreach (var sprite in clip.frames)
                            frameBuffer.Add(new SpriteFrameElement { uv = SpriteToUV(sprite) });
                    }
                    frameOffset += clip.isOverride ? 0 : (clip.frames?.Count ?? 0);
                }
            }

            var initSprite = authoring.animations[authoring.initialAnimation].frames[0];
            AddComponent(entity, new SpriteUVRect { value = SpriteToUV(initSprite) });
            AddComponent(entity, new OneShotAnimTag());
            SetComponentEnabled<OneShotAnimTag>(entity, false);
            AddComponent(entity, new CameraFacingData { invRotation = math.inverse(quaternion.RotateY(math.radians(authoring.cameraYAngle))) });
        }

        static float4 SpriteToUV(Sprite sprite)
        {
            Rect    r  = sprite.rect;
            Vector2 ts = sprite.texture.texelSize;
            return new float4(r.xMin * ts.x, r.yMin * ts.y, r.width * ts.x, r.height * ts.y);
        }
    }
}

[Serializable]
public class SpriteAnimationClip
{
    public string       name;
    public List<Sprite> frames;
    public float        fps        = 8f;
    public bool         isOverride = false;
    public int          overrideTo = 0;
    public bool         flip       = false;
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
    public int   overrideTo;
}

[InternalBufferCapacity(16)]
public struct SpriteFrameElement : IBufferElementData
{
    public float4 uv;
}

[MaterialProperty("_SpriteUV")]
public struct SpriteUVRect : IComponentData
{
    public float4 value;
}

public struct CameraFacingData : IComponentData
{
    public quaternion invRotation;
}
