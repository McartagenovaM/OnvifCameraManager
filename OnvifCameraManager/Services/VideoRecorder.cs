using OpenCvSharp;                         // OpenCV wrapper for .NET
using System;
using System.IO;
using OnvifCameraManager.Services;         // Required for OverlayHelper

namespace OnvifCameraManager.Services
{
    /// <summary>
    /// Static utility class that records a short video segment from an RTSP stream.
    /// </summary>
    public static class VideoRecorder
    {
        /// <summary>
        /// Records a fixed-length video segment from a given RTSP source.
        /// </summary>
        /// <param name="rtspUrl">RTSP URL with embedded credentials</param>
        /// <param name="outputDir">Directory where the video will be saved</param>
        /// <param name="seconds">Duration of the recorded segment in seconds (default = 10)</param>
        public static void RecordSegment(string rtspUrl, string outputDir, int seconds = 10)
        {
            // Ensure the output directory exists
            Directory.CreateDirectory(outputDir);

            // Open the RTSP video stream
            using var capture = new VideoCapture(rtspUrl);
            if (!capture.IsOpened())
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("RTSP stream not available for recording.");
                Console.ResetColor();
                return;
            }

            // Extract stream properties (FPS, resolution)
            int fps = (int)capture.Fps;
            int width = capture.FrameWidth;
            int height = capture.FrameHeight;
            int frameCount = fps * seconds;

            // Generate a timestamped filename
            string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
            string filePath = Path.Combine(outputDir, $"motion_{timestamp}.avi");

            // Initialize the video writer with MJPG codec
            using var writer = new VideoWriter(
                filePath,
                FourCC.MJPG,
                fps,
                new Size(width, height)
            );

            if (!writer.IsOpened())
            {
                Console.WriteLine("Could not open VideoWriter.");
                return;
            }

            Console.WriteLine("Recording video segment...");

            // Frame capture loop
            using var frame = new Mat();
            for (int i = 0; i < frameCount; i++)
            {
                if (!capture.Read(frame) || frame.Empty())
                    break;

                // Apply logo and text overlays using shared helper
                OverlayHelper.ApplyLogoAndText(frame);

                // Write frame to output video
                writer.Write(frame);
            }

            // Final output confirmation
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Video segment saved: {filePath}");
            Console.ResetColor();

            capture.Release();
        }
    }
}
