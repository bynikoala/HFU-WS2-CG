using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static System.Math;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Tutorial.Core
{
    public class AssetsPicking : RenderCanvas
    {
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        private ScenePicker _scenePicker;
        private PickResult _currentPick;
        private float3 _oldColor;
        private TransformComponent _baseTransform;
        private TransformComponent canon;
        private TransformComponent vr;
        private TransformComponent hr;
        private TransformComponent vl;        
        private TransformComponent hl;

        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to white (100% intentsity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0.2f, 0.2f, 0.2f, 1);

            _scene = AssetStorage.Get<SceneContainer>("vehicle.fus");

            _baseTransform = _scene.Children.FindNodes(node => node.Name == "vehicle")?.FirstOrDefault()?.GetTransform();
            canon = _scene.Children.FindNodes(node => node.Name == "canon")?.FirstOrDefault()?.GetTransform();
            vr = _scene.Children.FindNodes(node => node.Name == "vr")?.FirstOrDefault()?.GetTransform();
            hr = _scene.Children.FindNodes(node => node.Name == "hr")?.FirstOrDefault()?.GetTransform();
            vl = _scene.Children.FindNodes(node => node.Name == "vl")?.FirstOrDefault()?.GetTransform();            
            hl = _scene.Children.FindNodes(node => node.Name == "hl")?.FirstOrDefault()?.GetTransform();

            // Create a scene renderer holding the scene above
            _sceneRenderer = new SceneRenderer(_scene);

            _scenePicker = new ScenePicker(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            _baseTransform.Rotation = new float3(0, _baseTransform.Rotation.y + Keyboard.ADAxis * DeltaTime * 3, 0);
            canon.Rotation = new float3(0, canon.Rotation.y + Keyboard.LeftRightAxis * DeltaTime * 3, 0);
            vr.Rotation = new float3(vr.Rotation.x + Keyboard.WSAxis * DeltaTime * 3, 0, 0);
            hr.Rotation = new float3(hr.Rotation.x + Keyboard.WSAxis * DeltaTime * 3, 0, 0);
            vl.Rotation = new float3(vl.Rotation.x + Keyboard.WSAxis * DeltaTime * 3, 0, 0);
            hl.Rotation = new float3(hl.Rotation.x + Keyboard.WSAxis * DeltaTime * 3, 0, 0);

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, 0, 40) * float4x4.CreateRotationX(-0.7f) * float4x4.CreateRotationY(1.3f);

            if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);
                _scenePicker.View = RC.View;
                _scenePicker.Projection = RC.Projection;
                
                List<PickResult> pickResults = _scenePicker.Pick(pickPosClip).ToList();
            
                PickResult newPick = null;
                
                if (pickResults.Count > 0)
                {
                    pickResults.Sort((a, b) => Sign(a.ClipPos.z - b.ClipPos.z));
                    newPick = pickResults[0];
                }
                if (newPick?.Node != _currentPick?.Node)
                {
                    if (_currentPick != null)
                    {
                        _currentPick.Node.GetMaterial().Diffuse.Color = _oldColor;
                    }
                    if (newPick != null)
                    {
                        var mat = newPick.Node.GetMaterial();
                        _oldColor = mat.Diffuse.Color;
                        mat.Diffuse.Color = new float3(1, 0.4f, 0.4f);
                    }
                    _currentPick = newPick;
                }
            }

            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45ï¿½ Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
