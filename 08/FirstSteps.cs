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
    public class FirstSteps : RenderCanvas
    {
        private float _camAngle = 0;
        private TransformComponent _cubeTransform;

        private TransformComponent _cube2;
        private TransformComponent _cube3;
        private SceneContainer _scene;
        private SceneRenderer _sceneRenderer;
        // Init is called on startup. 
        public override void Init()
        {
            // Set the clear color for the backbuffer to light green (intensities in R, G, B, A).
            RC.ClearColor = new float4(0.2f, 0.2f, 0.2f, 1.0f);

        // Create a scene with a cube
        // The three components: one XForm, one Material and the Mesh
        _cubeTransform = new TransformComponent {Scale = new float3(1, 0.3f, 1), Translation = new float3(0, 0, 0)};
        _cube2= new TransformComponent {Scale = new float3(1, 0.3f, 1), Translation = new float3(0, 10, 0)};
        _cube3= new TransformComponent {Scale = new float3(1, 0.3f, 1), Translation = new float3(0, -10, 0)};

        var cubeMaterial = new MaterialComponent {
            Diffuse = new MatChannelContainer {Color = new float3(1, 0.7f, 0.2f)},
            Specular = new SpecularChannelContainer {Color = float3.One, Shininess = 4}
        };
        var cubeMesh = SimpleMeshes.CreateCuboid(new float3(10, 10, 10));

        // Assemble the cube nodes containing the three components
        var cubeNode = new SceneNodeContainer();
        cubeNode.Components = new List<SceneComponentContainer>();
        cubeNode.Components.Add(_cubeTransform);
        cubeNode.Components.Add(cubeMaterial);
        cubeNode.Components.Add(cubeMesh);

        var cubeNode2 = new SceneNodeContainer();
        cubeNode2.Components = new List<SceneComponentContainer>();
        cubeNode2.Components.Add(_cube2);
        cubeNode2.Components.Add(cubeMaterial);
        cubeNode2.Components.Add(cubeMesh);

        var cubeNode3 = new SceneNodeContainer();
        cubeNode3.Components = new List<SceneComponentContainer>();
        cubeNode3.Components.Add(_cube3);
        cubeNode3.Components.Add(cubeMaterial);
        cubeNode3.Components.Add(cubeMesh);

        // Create the scene containing the cube as the only object
        _scene = new SceneContainer();
        _scene.Children = new List<SceneNodeContainer>();
        _scene.Children.Add(cubeNode);
        _scene.Children.Add(cubeNode2);
        _scene.Children.Add(cubeNode3);

        // Create a scene renderer holding the scene above
        _sceneRenderer = new SceneRenderer(_scene);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Animate the cube
            _cubeTransform.Translation = new float3(0, 5 * M.Sin(3 * TimeSinceStart), 0);

            // Animate the cube-scaling
            _cube2.Scale = new float3(3 * M.Sin(0.5f * TimeSinceStart), 0.3f, 3 * M.Sin(0.5f * TimeSinceStart));
            _cube3.Scale = new float3(3 * M.Sin(0.5f * TimeSinceStart), 0.3f, 3 * M.Sin(0.5f * TimeSinceStart));

            // Animate the colors
            for (int i=1; i<3; i++) {
                _scene.Children[i].Components[1] = new MaterialComponent {
                    Diffuse = new MatChannelContainer {Color = new float3(M.Cos(TimeSinceStart), M.Sin(TimeSinceStart), M.Cos(TimeSinceStart))},
                    Specular = new SpecularChannelContainer {Color = float3.One, Shininess = 4}
                    };
            }
            

            // Animate the camera angle
            _camAngle = _camAngle + 90.0f * M.Pi/180.0f * DeltaTime;

            // Setup the camera 
            RC.View = float4x4.CreateTranslation(0, 0, 50) * float4x4.CreateRotationY(_camAngle);

            // Render the scene on the current render context
            _sceneRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();            
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }
    }
}
