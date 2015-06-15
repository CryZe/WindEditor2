﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem
    {
        private List<Camera> m_cameraList;
        private ShaderProgram m_shader;
        private Mesh m_testMesh;

        private List<Mesh> m_meshList;

        public RenderSystem()
        {
            m_cameraList = new List<Camera>();
            m_meshList = new List<Mesh>();
            m_shader = new ShaderProgram("RenderSystem/Shaders/vert.glsl", "RenderSystem/Shaders/frag.glsl");

            // Create a Default camera
            //Camera editorCamera = new Camera();
            //m_cameraList.Add(editorCamera);

            Camera leftCamera = new Camera();
            leftCamera.ClearColor = new Color(1f, 0.5f, 0, 1f);
            leftCamera.ViewportRect = new Rect(0f, 0f, 0.5f, 1f);


            Camera rightCamera = new Camera();
            rightCamera.ViewportRect = new Rect(0.5f, 0f, 0.5f, 1f);
            rightCamera.ClearColor = new Color(0.5f, 0, 1f, 1f);

            m_cameraList.Add(leftCamera);
            m_cameraList.Add(rightCamera);

            /* Create a default cube */
            m_testMesh = new Mesh();
            Vector3 size = new Vector3(2f, 2f, 2f);

            Vector3[] meshVerts =
            {
                new Vector3(-size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  -size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  -size.Z / 2f),
                new Vector3(-size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, -size.Y / 2f,  size.Z / 2f),
                new Vector3(size.X / 2f, size.Y / 2f,  size.Z / 2f),
                new Vector3(-size.X / 2f, size.Y / 2f,  size.Z / 2f),
            };

            int[] meshIndexes =
            {
                //front
                0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };

            m_testMesh.Vertices = meshVerts;
            m_testMesh.Indexes = meshIndexes;

            Color[] colors = new Color[meshVerts.Length];
            for (int i = 0; i < meshVerts.Length; i++)
                colors[i] = new Color(1f, 1f, 0f, 0.5f);
            m_testMesh.Color0 = colors;

            m_meshList.Add(m_testMesh);
        }

        internal void RenderFrame()
        {
            // Solid Fill the Back Buffer, until I can figure out what's going on with resizing
            // windows and partial camera viewport rects.
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.DepthTest);
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                /* SETUP THE VIEWPORT FOR THE CAMERA */
                Camera camera = m_cameraList[i];

                Rect pixelRect = camera.PixelRect;
                GL.Viewport((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);
                GL.Scissor((int)pixelRect.X, (int)pixelRect.Y, (int)pixelRect.Width, (int)pixelRect.Height);

                // Clear the backbuffer
                Color clearColor = camera.ClearColor;
                GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
                GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);

                Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjectionMatrix;

                for (int m = 0; m < m_meshList.Count; m++)
                {
                    Mesh mesh = m_meshList[m];

                    // Bind the Shader
                    GL.UseProgram(m_shader.ProgramId);


                    Matrix4 modelMatrix = Matrix4.Identity; //Identity = doesn't change anything when multiplied.
                    Matrix4 finalMatrix = modelMatrix * viewProjMatrix;

                    // Bind the VAOs currently associated with this Mesh
                    mesh.Bind();

                    // Upload the MVP to the GPU
                    GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);

                    // Draw our Mesh.
                    GL.DrawElements(PrimitiveType.Triangles, mesh.Indexes.Length, DrawElementsType.UnsignedInt, 0);
                    
                    // Unbind the VAOs so that our VAO doesn't leak into the next drawcall.
                    mesh.Unbind();
                }
            }
            GL.Disable(EnableCap.ScissorTest);
            GL.Disable(EnableCap.DepthTest);

            //  Flush OpenGL.
            GL.Flush();
        }

        internal void SetOutputSize(float width, float height)
        {
            // Re-Calculate perspective camera ratios here.
            for (int i = 0; i < m_cameraList.Count; i++)
            {
                Camera camera = m_cameraList[i];
                camera.PixelWidth = width;
                camera.PixelHeight = height;
            }
        }
    }
}