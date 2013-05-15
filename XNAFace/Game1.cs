using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace XNAFace
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        /// <summary>
        /// This is used to adjust the window width.
        /// </summary>
        private const int Width = 800;
        
        /// <summary>
        /// This is used to adjust the window height.
        /// </summary>
        private const int Height = 600;

        /// <summary>
        /// Resolution of the color stream.
        /// </summary>
        private const ColorImageFormat colorImageFormat = ColorImageFormat.RgbResolution640x480Fps30;

        /// <summary>
        /// Resolution of the depth stream.
        /// </summary>
        private const DepthImageFormat depthImageFormat = DepthImageFormat.Resolution640x480Fps30;
        
        /// <summary>
        /// Color data from Kinect Sensor.
        /// </summary>
        private byte[] colorData;

        /// <summary>
        /// Depth data from Kinect Sensor.
        /// </summary>
        private short[] depthData;
 
        /// <summary>
        /// Nearset skeleton from Kinect Sensor.
        /// </summary>
        Skeleton nearestSkeleton;

        /// <summary>
        /// Loaded model of glasses.
        /// </summary>
        Model glasses;

        /// <summary>
        /// This control selects a sensor, and displays a notice if one is
        /// not connected.
        /// </summary>
        private readonly KinectChooser chooser;
        
        /// <summary>
        /// For more information see Microsoft.Kinect.Toolkit.FaceTracking project.
        /// </summary>
        private FaceTracker faceTracker;

        /// <summary>
        /// This manages the rendering of the color stream.
        /// </summary>
        private readonly ColorStreamRenderer colorStream;

        /// <summary>
        /// This manages the rendering of the depth stream.
        /// </summary>
        private readonly DepthStreamRenderer depthStream;

        /// <summary>
        /// This manages the rendering of the skeleton stream.
        /// </summary>
        private readonly SkeletonStreamRenderer skeletonStream;

        /// <summary>
        /// This is the viewport of the streams.
        /// </summary>
        private readonly Rectangle viewPortRectangle;

        float yaw, pitch, roll;
        float scale;
        Vector3 vector;


        public Game1()
        {
            this.IsFixedTimeStep = false;
            this.IsMouseVisible = true;
            this.Window.Title = "Xna Basics";

            // This sets the width to the desired width
            // It also forces a 4:3 ratio for height
            this.graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = Width;
            this.graphics.PreferredBackBufferHeight = Height;
            this.graphics.SynchronizeWithVerticalRetrace = true;
            this.viewPortRectangle = new Rectangle(0, 0, Width, Height);

            Content.RootDirectory = "Content";

            // The Kinect sensor will use 640x480 for both streams
            // To make your app handle multiple Kinects and other scenarios,
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            this.chooser = new KinectChooser(this, colorImageFormat, depthImageFormat);
            this.Services.AddService(typeof(KinectChooser), this.chooser);

            // Default size is the full viewport
            this.colorStream = new ColorStreamRenderer(this);
            this.depthStream = new DepthStreamRenderer(this);
            this.skeletonStream = new SkeletonStreamRenderer(this);

            this.Components.Add(this.chooser);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.Components.Add(this.colorStream);
            this.Components.Add(this.depthStream);
            this.Components.Add(this.skeletonStream); 
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            this.Services.AddService(typeof(SpriteBatch), this.spriteBatch);
            // TODO: use this.Content to load your game content here
            glasses = Content.Load<Model>("glasses");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            colorData = colorStream.ColorData;
            depthData = depthStream.DepthData;
            nearestSkeleton = skeletonStream.Skel;

            if (nearestSkeleton != null && nearestSkeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(this.chooser.Sensor);
                    }
                    catch (InvalidOperationException)
                    {
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    FaceTrackFrame faceTrackFrame = this.faceTracker.Track(
                        colorImageFormat,
                        colorData,
                        depthImageFormat,
                        depthData,
                        nearestSkeleton);

                    if (faceTrackFrame.TrackSuccessful)
                    {
                        EnumIndexableCollection<FeaturePoint, Vector3DF> shapePoints = faceTrackFrame.Get3DShape();
                        EnumIndexableCollection<FeaturePoint, PointF> projectedShapePoints = faceTrackFrame.GetProjected3DShape();

                        yaw = -MathHelper.ToRadians(faceTrackFrame.Rotation.Y);
                        pitch = -MathHelper.ToRadians(faceTrackFrame.Rotation.X);
                        roll = MathHelper.ToRadians(faceTrackFrame.Rotation.Z);

                        vector.X = 9.3f * (shapePoints[4].X / shapePoints[4].Z);
                        vector.Y = 9.3f * (shapePoints[4].Y / shapePoints[4].Z) * 0.95f;
                        vector.Z = 0;
                        scale = 0.4f;

                        Window.Title = shapePoints[4].X.ToString() + " " + shapePoints[4].Y.ToString() + " " + shapePoints[4].Z.ToString();
                    }
                    else
                        scale = 0;
                }
            }

            if (gameTime.TotalGameTime.Seconds > 3)
            {
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);

            // TODO: Add your drawing code here
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up);
            Matrix proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), GraphicsDevice.Viewport.AspectRatio, 0.1f, 15);
            Matrix[] transforms = new Matrix[glasses.Bones.Count];
            glasses.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in glasses.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = true;
                    effect.AmbientLightColor = new Vector3(0.8f, 0.8f, 0.8f);
                    effect.DirectionalLight0.Enabled = true;
                    effect.DirectionalLight0.Direction = new Vector3(0, 0, 1);
                    effect.DirectionalLight0.DiffuseColor = new Vector3(0, 1, 0);
                    effect.DirectionalLight0.SpecularColor = new Vector3(1, 1, 1);

                    effect.World = transforms[mesh.ParentBone.Index] 
                        * Matrix.CreateScale(scale)
                        * Matrix.CreateTranslation(0, 0, -0.75f)
                        * Matrix.CreateFromYawPitchRoll(yaw, pitch, roll)
                        * Matrix.CreateTranslation(vector);  
                    effect.View = view;
                    effect.Projection = proj;
                }
                mesh.Draw();
            }
        }
    }
}
