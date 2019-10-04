using System;
using WaveEngine.Common.Graphics;
using WaveEngine.Mathematics;

namespace TextureCubeToSpherical
{
    public class SphericalProjectionTest : BaseTest
    {
        private Viewport[] viewports;
        private Rectangle[] scissors;
        private CommandQueue graphicsCommandQueue;
        private GraphicsPipelineState graphicsPipelineState;
        private ResourceSet resourceSet;
        private uint width;
        private uint height;

        public SphericalProjectionTest()
            : base("TextureCubeToSpherical")
        {
        }

        protected override async void InternalLoad()
        {
            var swapChainDescription = this.swapChain?.SwapChainDescription;
            this.width = swapChainDescription.Value.Width;
            this.height = swapChainDescription.Value.Height;

            // Graphics Resources
            var vertexShaderDescription = await this.assetsDirectory.ReadAndCompileShader(this.graphicsContext, "HLSL", "VertexShader", ShaderStages.Vertex, "VS");
            var pixelShaderDescription = await this.assetsDirectory.ReadAndCompileShader(this.graphicsContext, "HLSL", "FragmentShader", ShaderStages.Pixel, "PS");
            var vertexShader = this.graphicsContext.Factory.CreateShader(ref vertexShaderDescription);
            var pixelShader = this.graphicsContext.Factory.CreateShader(ref pixelShaderDescription);

            Texture textureCube = null;
            using (var stream = System.IO.File.OpenRead("Content/TextureCubeToSpherical/TextureCube.ktx"))
            {
                if (stream != null)
                {
                    VisualTests.LowLevel.Images.Image image = VisualTests.LowLevel.Images.Image.Load(stream);
                    var textureDescription = image.TextureDescription;
                    textureCube = graphicsContext.Factory.CreateTexture(image.DataBoxes, ref textureDescription);
                }
            }

            var samplerDescription = SamplerStates.LinearClamp;
            var samplerState = this.graphicsContext.Factory.CreateSamplerState(ref samplerDescription);

            ResourceLayoutDescription layoutDescription = new ResourceLayoutDescription(
                   new LayoutElementDescription(0, ResourceType.Texture, ShaderStages.Pixel),
                   new LayoutElementDescription(0, ResourceType.Sampler, ShaderStages.Pixel));
            ResourceLayout resourceLayout = this.graphicsContext.Factory.CreateResourceLayout(ref layoutDescription);

            ResourceSetDescription resourceSetDescription = new ResourceSetDescription(resourceLayout, textureCube, samplerState);
            this.resourceSet = this.graphicsContext.Factory.CreateResourceSet(ref resourceSetDescription);

            var pipelineDescription = new GraphicsPipelineDescription()
            {
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                InputLayouts = null,
                ResourceLayouts = new[] { resourceLayout },
                Shaders = new ShaderStateDescription()
                {
                    VertexShader = vertexShader,
                    PixelShader = pixelShader,
                },
                RenderStates = new RenderStateDescription()
                {
                    RasterizerState = RasterizerStates.CullBack,
                    BlendState = BlendStates.Opaque,
                    DepthStencilState = DepthStencilStates.Read,
                },
                Outputs = this.frameBuffer.OutputDescription,
            };

            this.graphicsPipelineState = this.graphicsContext.Factory.CreateGraphicsPipeline(ref pipelineDescription);
            this.graphicsCommandQueue = this.graphicsContext.Factory.CreateCommandQueue(CommandQueueType.Graphics);

            this.viewports = new Viewport[1];
            this.viewports[0] = new Viewport(0, 0, this.width, this.height);
            this.scissors = new Rectangle[1];
            this.scissors[0] = new Rectangle(0, 0, (int)this.width, (int)this.height);
        }

        protected override void InternalDrawCallback(TimeSpan gameTime)
        {
            var graphicsCommandBuffer = this.graphicsCommandQueue.CommandBuffer();
            graphicsCommandBuffer.Begin();

            RenderPassDescription renderPassDescription = new RenderPassDescription(this.frameBuffer, ClearValue.None);
            graphicsCommandBuffer.BeginRenderPass(ref renderPassDescription);

            graphicsCommandBuffer.SetViewports(this.viewports);
            graphicsCommandBuffer.SetScissorRectangles(this.scissors);
            graphicsCommandBuffer.SetGraphicsPipelineState(this.graphicsPipelineState);
            graphicsCommandBuffer.SetResourceSet(this.resourceSet);
            graphicsCommandBuffer.Draw(3);

            graphicsCommandBuffer.EndRenderPass();
            graphicsCommandBuffer.End();

            graphicsCommandBuffer.Commit();

            this.graphicsCommandQueue.Submit();
            this.graphicsCommandQueue.WaitIdle();
        }
    }
}
