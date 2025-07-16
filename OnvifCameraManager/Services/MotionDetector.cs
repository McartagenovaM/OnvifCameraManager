using OpenCvSharp;             // OpenCV wrapper for .NET - provides image/video processing APIs
using System;
using System.Threading.Tasks;

namespace OnvifCameraManager.Services
{
    /// <summary>
    /// Static class for detecting motion between consecutive RTSP video frames.
    /// </summary>
    public static class MotionDetector
    {
        // Stores the previous frame in grayscale for comparison
        private static Mat? previousGray = null;

        /// <summary>
        /// Compares the current frame from the RTSP stream to the previous frame to detect motion.
        /// </summary>
        // <param name="rtspUrl">RTSP URL with embedded credentials</param>
        /// <returns>True if motion is detected, otherwise false</returns>
        public static bool DetectMotion(string rtspUrl)
        {
            // Open the video stream
            using var capture = new VideoCapture(rtspUrl);
            if (!capture.IsOpened())
            {
                // Print error if stream can't be accessed
                Console.WriteLine("RTSP stream unavailable.");
                return false;
            }

            // Read a valid frame from the stream (try up to 10 times)
            using var frame = new Mat();
            for (int i = 0; i < 10; i++)
            {
                if (capture.Read(frame) && !frame.Empty())
                    break;
            }

            // Convert the captured frame to grayscale for easier analysis
            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

            // Default: assume no motion
            bool motion = false;

            // If we already have a previous frame to compare with
            if (previousGray != null)
            {
                // Compute pixel-wise absolute difference between current and previous grayscale frame
                using var diff = new Mat();
                Cv2.Absdiff(previousGray, gray, diff);

                // Apply binary threshold to emphasize changed areas
                Cv2.Threshold(diff, diff, 25, 255, ThresholdTypes.Binary);

                // Count how many pixels changed significantly
                int changes = Cv2.CountNonZero(diff);

                // Decide if motion is significant based on pixel threshold
                motion = changes > 250000; // Sensitivity: adjust this value based on resolution and test

                if (motion)
                    Console.WriteLine($"Motion detected: {changes} pixels changed.");
                else
                    Console.WriteLine("No motion detected.");
            }
            else
            {
                // First frame ever read — no comparison yet
                Console.WriteLine("Initialized frame buffer.");
            }

            // Store current grayscale frame as the previous one for next comparison
            previousGray = gray.Clone();

            return motion;
        }
    }
}
