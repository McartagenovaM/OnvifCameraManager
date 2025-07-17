using OpenCvSharp;
using System;
using System.IO;

namespace OnvifCameraManager.Services
{
    public static class OverlayHelper
    {
        /// <summary>
        /// Applies both a semi-transparent image overlay and a text label to a video frame.
        /// </summary>
        /// <param name="frame">The video frame to decorate</param>
        public static void ApplyLogoAndText(Mat frame)
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Owl.png");
            using var logo = Cv2.ImRead(logoPath, ImreadModes.Unchanged);

            // Overlay logo in bottom-right
            if (!logo.Empty() && logo.Channels() == 4)
            {
                int x = frame.Width - logo.Width - 10;
                int y = frame.Height - logo.Height - 10;

                var roi = new Rect(x, y, logo.Width, logo.Height);
                var frameRoi = new Mat(frame, roi);

                using var logoBgr = new Mat();
                Cv2.CvtColor(logo, logoBgr, ColorConversionCodes.BGRA2BGR);
                Cv2.AddWeighted(logoBgr, 0.5, frameRoi, 0.5, 0, frameRoi);
            }

            // Add static text in bottom-left
            string text = "M Cartagenova - 2025";
            var position = new Point(10, frame.Height - 20);
            Cv2.PutText(frame, text, position, HersheyFonts.HersheySimplex, 1.0, Scalar.White, 2);
        }
    }
}