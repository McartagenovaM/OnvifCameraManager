using OpenCvSharp;
using System;
using System.IO;

public static class FrameCapture
{
    // Store the previous grayscale frame for comparison
    private static Mat? previousFrame = null;

    public static void CaptureAndSaveFrame(string rtspUrl)
    {
        // Define output directory for saved frames
        string outputDir = @"C:\temp\capture";
        Directory.CreateDirectory(outputDir); // Create it if it doesn't exist

        // Initialize RTSP stream capture
        using var capture = new VideoCapture(rtspUrl);
        if (!capture.IsOpened())
        {
            // Show red message if stream couldn't be opened
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("RTSP stream not available.");
            Console.ResetColor();
            return;
        }

        // Try to grab a valid frame (some streams need a few reads to warm up)
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
            // Warn if no valid frame was captured
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Cannot capture frames.");
            Console.ResetColor();
            return;
        }

        // Convert captured frame to grayscale to simplify comparison
        using var gray = new Mat();
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

        bool motionDetected = false;

        if (previousFrame != null)
        {
            // Compute absolute pixel-wise difference between current and previous frame
            using var diff = new Mat();
            Cv2.Absdiff(previousFrame, gray, diff);

            // Optional: apply binary threshold to highlight significant changes
            Cv2.Threshold(diff, diff, 25, 255, ThresholdTypes.Binary);

            // Count number of changed pixels
            int nonZeroCount = Cv2.CountNonZero(diff);

            // If number of changed pixels exceeds threshold → motion detected
            if (nonZeroCount > 245000) // Adjust sensitivity based on resolution and needs
            {
                motionDetected = true;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Motion detected: ");
                Console.ResetColor();
                Console.WriteLine($"{nonZeroCount} pixels changed.");
            }
        }
        else
        {
            // Always save the very first frame (no previous to compare)
            motionDetected = true;
            Console.WriteLine("First frame — saved by default.");
        }

        if (motionDetected)
        {
            // Save the frame to disk with timestamped name
            string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
            string fileName = $"capture_{timestamp}.jpg";
            string fullPath = Path.Combine(outputDir, fileName);

            // Uncomment the next line to save the frame without compression
            //Cv2.ImWrite(fullPath, frame);

            //Save the frame as JPEG with reduced quality to save space
            Cv2.ImWrite(fullPath, frame, new int[] { (int)ImwriteFlags.JpegQuality, 25 });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Frame saved: ");
            Console.ResetColor();
            Console.WriteLine($"{fullPath}");
        }
        else
        {
            // No motion — frame skipped
            Console.WriteLine("No motion detected — frame ignored.");
        }

        // Save current frame for next comparison
        previousFrame = gray.Clone();

        // Release the video capture
        capture.Release();
    }
}
