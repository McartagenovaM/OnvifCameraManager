using OpenCvSharp;
using System;
using System.IO;

public static class FrameCapture
{
    public static void CaptureAndSaveFrame(string rtspUrl)
    {
        string outputDir = @"C:\temp\capture";
        Directory.CreateDirectory(outputDir);

        string timestamp = DateTime.Now.ToString("ddMMyyHHmmss");
        string fileName = $"capture_{timestamp}.jpg";
        string fullPath = Path.Combine(outputDir, fileName);

        using var capture = new VideoCapture(rtspUrl);
        if (!capture.IsOpened())
        {
            Console.WriteLine("RTSP stream not available.");
            return;
        }

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

        if (readSuccess)
        {
            Cv2.ImWrite(fullPath, frame);
            Console.WriteLine($"Frame saved: {fullPath}");
        }
        else
        {
            Console.WriteLine("Cannot capture frames. ");
        }

        capture.Release();
    }
}
