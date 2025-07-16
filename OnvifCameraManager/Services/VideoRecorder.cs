using OpenCvSharp;           // OpenCV wrapper for .NET: enables video frame capture and processing
using System;
using System.IO;

namespace OnvifCameraManager.Services
{
    /// <summary>
    /// Static utility class that records a short video segment from an RTSP stream.
    /// </summary>
    public static class VideoRecorder
    {
        // <summary>
        // Records a fixed-length video segment from a given RTSP source.
        // </summary>
        // <param name="rtspUrl">RTSP URL with embedded credentials</param>
        // <param name="outputDir">Directory where the video will be saved</param>
        // <param name="seconds">Duration of the recorded segment in seconds (default = 10)</param>
        public static void RecordSegment(string rtspUrl, string outputDir, int seconds = 10)
        {
            // Ensure the output directory exists
            Directory.CreateDirectory(outputDir);

            // Open the video stream
            using var capture = new VideoCapture(rtspUrl);
            if (!capture.IsOpened())
            {
                // Error if stream can't be opened
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("RTSP stream not available for recording.");
                Console.ResetColor();
                return;
            }

            // Get video stream properties
            int fps = (int)capture.Fps;                     // Frames per second
            int width = capture.FrameWidth;                // Frame width in pixels
            int height = capture.FrameHeight;              // Frame height in pixels
            int frameCount = fps * seconds;                // Total number of frames to record

            // Generate a timestamped filename
            string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
            string filePath = Path.Combine(outputDir, $"motion_{timestamp}.avi");

            // Initialize the video writer using MJPG codec
            using var writer = new VideoWriter(
                filePath,        // Output file path
                FourCC.MJPG,     // Codec
                fps,             // Frame rate
                new Size(width, height) // Frame size
            );

            // Check if writer is ready
            if (!writer.IsOpened())
            {
                Console.WriteLine("Could not open VideoWriter.");
                return;
            }

            Console.WriteLine("Recording video segment...");

            // Loop to capture and write each frame
            using var frame = new Mat();
            for (int i = 0; i < frameCount; i++)
            {
                if (!capture.Read(frame) || frame.Empty())
                    break;  // Stop recording if frame is not available

                writer.Write(frame); // Write frame to output video
            }

            // Final confirmation
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Video segment saved: {filePath}");
            Console.ResetColor();

            capture.Release(); // Clean up video stream
        }
    }
}
