using System;
using System.Threading;
using System.Threading.Tasks;
using OnvifCameraManager.Services;
using SharpOnvifClient;

class Program
{
    static async Task Main()
    {
        Console.Clear();
        // ─── ASCII banner ──────────────────────────────────────────────────────


        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║            Onvif Discovery and Capture           ║");
        Console.WriteLine("║    (using SharpOnvifClient and OpenCvSharp4)     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Discovering ONVIF cameras on the network...");
        Console.WriteLine();

        Environment.SetEnvironmentVariable("OPENCV_FFMPEG_DEBUG", "0");
        Environment.SetEnvironmentVariable("OPENCV_VIDEOIO_DEBUG", "0");
        Environment.SetEnvironmentVariable("OPENCV_LOG_LEVEL", "OFF");


        var devices = await OnvifDiscoveryClient.DiscoverAsync();

        if (devices.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("No ONVIF cameras found.");
            Console.ResetColor();
            return;
        }

        for (int i = 0; i < devices.Count; i++)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{i + 1}: ");
            Console.ResetColor();
            Console.WriteLine($"{devices[i].Addresses[0]}");
        }
        Console.WriteLine();

        Console.Write("\nSelect a camera (number): ");
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > devices.Count)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Invalid selection.");
            Console.ResetColor();
            return;
        }
        Console.WriteLine();

        var dev = devices[idx - 1];
        string serviceUrl = dev.Addresses[0];

        Console.Write("Camera Username: ");
        string user = Console.ReadLine();

        Console.Write("Camera Password: ");
        string pass = Console.ReadLine();
        Console.WriteLine();

        SimpleOnvifClient client;
        try
        {
            client = new SimpleOnvifClient(serviceUrl, user!, pass!);
            Console.WriteLine("Initializing ONVIF client...");
            Console.WriteLine();

            // Get brand and model
            var deviceInfo = await client.GetDeviceInformationAsync();
            Console.WriteLine($"Connected to Camera: {deviceInfo.Manufacturer} {deviceInfo.Model}\n");


            var profilesResponse = await client.GetProfilesAsync();
           //Console.WriteLine($"Found {profilesResponse.Profiles.Length} profile(s).");

            var uri = await client.GetStreamUriAsync(profilesResponse.Profiles[0].token);

            // Build RTSP URL with credentials
            var credentialManager = new CredentialManager();
            string fullRtspUrl = credentialManager.InsertCredentialsIntoRtspUrl(uri.Uri, user, pass);
            Console.WriteLine($"Using Camera RTSP URL: {fullRtspUrl}\n");


            Console.WriteLine("Select capture mode:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("1: ");
            Console.ResetColor();
            Console.WriteLine("Save frames every N seconds");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("2: ");
            Console.ResetColor();
            Console.WriteLine("Record 10-second video on motion detection");
            Console.WriteLine();
            Console.Write("Enter option: ");
            string option = Console.ReadLine();



            if (option == "1")
            {
                Console.WriteLine("Capturing frames every 5 seconds. Press ESC to stop.");

                var captureTask = Task.Run(() =>
                {
                    while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
                    {
                        FrameCapture.CaptureAndSaveFrame(fullRtspUrl);
                        Thread.Sleep(5000); // You can adjust interval
                    }
                });

                await captureTask;
            }
            else if (option == "2")
            {
                Console.WriteLine("Monitoring for motion. Press ESC to stop.");

                var motionTask = Task.Run(() =>
                {
                    while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
                    {
                        if (MotionDetector.DetectMotion(fullRtspUrl))
                        {
                            VideoRecorder.RecordSegment(fullRtspUrl, @"C:\temp\capture");
                        }

                        Thread.Sleep(5000); // Detection interval
                    }
                });

                await motionTask;
            }
            else
            {
                Console.WriteLine("Invalid option selected. Exiting.");
            }



        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed; 
            Console.WriteLine();
            Console.WriteLine($"Unable to initialize camera: {ex.Message}");
            Console.WriteLine("Please verify credentials and try again.");
            Console.ResetColor();
        }
    }
}
