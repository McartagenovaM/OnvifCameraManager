using OpenCvSharp;                        // OpenCV wrapper for .NET (image/video processing)
using System;
using System.IO;
using OnvifCameraManager.Services;       // Import OverlayHelper (external overlay logic)

public static class FrameCapture
{
    // Store previous grayscale frame for motion comparison
    private static Mat? previousGray = null;

    /// <summary>
    /// Captures a frame from an RTSP stream, detects motion, and saves it as JPEG if motion is detected.
    /// </summary>
    /// <param name="rtspUrl">RTSP stream URL including credentials</param>
    public static void CaptureAndSaveFrame(string rtspUrl)
    {
        // Output directory where captures will be saved
        string outputDir = @"C:\temp\capture";
        Directory.CreateDirectory(outputDir);

        // Open video stream
        using var capture = new VideoCapture(rtspUrl);
        if (!capture.IsOpened())
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("RTSP stream not available.");
            Console.ResetColor();
            return;
        }

        // Try to capture a valid frame (some cameras take a few reads to initialize)
        using var frame = new Mat();
        bool readSuccess = false;
        for (int i = 0; i < 10; i++)
        {
            if (capture.Read(frame) && !frame.Empty())
            {
                readSuccess = true;
                break;
            }
        }

        if (!readSuccess)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Cannot capture frames.");
            Console.ResetColor();
            return;
        }

        // Convert captured frame to grayscale for motion detection
        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

        bool motionDetected = false;

        // Compare with previous frame if available
        if (previousGray != null)
        {
            using var diff = new Mat();
            Cv2.Absdiff(previousGray, gray, diff);              // Pixel difference
            Cv2.Threshold(diff, diff, 25, 255, ThresholdTypes.Binary);  // Binarize for clarity

            int changes = Cv2.CountNonZero(diff);               // Count pixels that changed

            // Basic motion trigger logic based on pixel threshold
            motionDetected = changes > 250000; // You can fine-tune this value
            Console.ForegroundColor = motionDetected ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.WriteLine(motionDetected
                ? $"Motion detected: {changes} pixels changed."
                : "No motion detected  — frame ignored.");
            Console.ResetColor();
        }
        else
        {
            // First frame always triggers capture
            Console.WriteLine("Initialized frame buffer.");
            motionDetected = true;
        }

        if (motionDetected)
        {
            // Add overlays (logo + text) using external helper
            OverlayHelper.ApplyLogoAndText(frame);

            // Generate timestamped filename
            string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
            string fileName = $"capture_{timestamp}.jpg";
            string fullPath = Path.Combine(outputDir, fileName);

            // Save frame with JPEG quality compression set to 25%
            Cv2.ImWrite(fullPath, frame, new[] { (int)ImwriteFlags.JpegQuality, 25 });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Frame saved: ");
            Console.ResetColor();
            Console.WriteLine(fullPath);
        }

        // Store current grayscale for next motion comparison
        previousGray = gray.Clone();

        // Cleanup
        capture.Release();
    }
}
