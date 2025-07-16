# ONVIF Camera Capture Tool

This is a .NET console application that:

- Discovers ONVIF-compatible IP cameras on the network
- Authenticates via ONVIF protocol (username/password)
- Retrieves the RTSP stream URI
- Offers two capture modes:
  - Save image frames every N seconds
  - Record 10-second video segments when motion is detected

## Features

- ONVIF camera discovery using SharpOnvifClient
- RTSP video processing with OpenCvSharp4
- Frame difference-based motion detection
- Frame compression (JPEG quality setting)
- Video recording with H.264 or MJPEG codecs

## Requirements

- .NET 6 or higher
- OpenCvSharp4 + runtime
- FFmpeg-enabled OpenCV build for H.264/265 encoding (optional)

## Running the App

1. Clone the repository:
   git clone https://github.com/yourusername/onvif-capture-tool.git

2. Build and run:
  dotnet build
  dotnet run

3. Choose the camera, enter credentials, and select the capture mode.

Captured files are saved to C:\temp\capture.
