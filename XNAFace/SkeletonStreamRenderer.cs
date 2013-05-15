namespace XNAFace
{
    using System;
    using System.Linq;
    using Microsoft.Kinect;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// This class is responsible for rendering a skeleton stream.
    /// </summary>
    public class SkeletonStreamRenderer : Object2D
    {

        /// <summary>
        /// The last frames skeleton data.
        /// </summary>
        private static Skeleton[] skeletonData;
        private int trackingId = -1;
        public Skeleton Skel;

        /// <summary>
        /// Initializes a new instance of the SkeletonStreamRenderer class.
        /// </summary>
        /// <param name="game">The related game object.</param>
        /// <param name="map">The method used to map the SkeletonPoint to the target space.</param>
        public SkeletonStreamRenderer(Game game)
            : base(game)
        {

        }

        /// <summary>
        /// This method initializes necessary values.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// This method retrieves a new skeleton frame if necessary.
        /// </summary>
        /// <param name="gameTime">The elapsed game time.</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // If the sensor is not found, not running, or not connected, stop now
            if (null == this.Chooser.Sensor ||
                false == this.Chooser.Sensor.IsRunning ||
                KinectStatus.Connected != this.Chooser.Sensor.Status)
            {
                return;
            }

            // If we have already drawn this skeleton, then we should retrieve a new frame
            // This prevents us from calling the next frame more than once per update
                using (var skeletonFrame = this.Chooser.Sensor.SkeletonStream.OpenNextFrame(0))
                {
                    // Sometimes we get a null frame back if no data is ready
                    if (null == skeletonFrame)
                    {
                        return;
                    }

                    // Reallocate if necessary
                    if (null == skeletonData || skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    Skeleton skeletonOfInterest =
                    skeletonData.FirstOrDefault(
                            skeleton =>
                            skeleton.TrackingId == this.trackingId
                            && skeleton.TrackingState != SkeletonTrackingState.NotTracked);

                    if (skeletonOfInterest == null)
                    {
                        // Old one wasn't around.  Find any skeleton that is being tracked and use it.
                        skeletonOfInterest =
                            skeletonData.FirstOrDefault(
                                skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);

                        if (skeletonOfInterest != null)
                        {
                            this.trackingId = skeletonOfInterest.TrackingId;
                        }
                    }
                    Skel = skeletonOfInterest;

                    if (gameTime.TotalGameTime.Seconds > 3)
                    {
                    }
                }
            
        }

    }
}
