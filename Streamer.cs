using Intel.RealSense;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace iuF {
    public static class Streamer {

        /* CAMERA PROPERTIES */
        const ushort CAMERA_WIDTH = 640;
        const ushort CAMERA_HEIGHT = 480;
        const ushort CAMERA_FRAMERATE = 30;

        /* RESOLUTIONS (for debug display purpose) */
        const ushort RES_WIDTH = 10; // 10px width => 1px width
        const ushort RES_HEIGHT = 20; // 20px height => 1px height
        const ushort RES_DEPTH = 1500; // maximum depth of display

        /* Extract and return the Depth data from the Frame */
        private static ushort[] DepthArray(VideoFrame depthFrame) {
            ushort[] depthArray = new ushort[CAMERA_WIDTH * CAMERA_HEIGHT];
            depthFrame.CopyTo(depthArray);
            return depthArray; 
        }

        /* Extract and return the Vertices datas from the Frame */
        private static float[] VerticeArray(VideoFrame depthFrame) {
            PointCloud pointCloud = new PointCloud();
            Points points = pointCloud.Process<VideoFrame>(depthFrame).As<Points>();
            float[] vertices = new float[points.Count * 3];
            points.CopyVertices(vertices);
            return vertices;
        }

        /* Extract and return the Color data from the Frame */
        private static byte[] ColorArray(VideoFrame colorFrame) {
            byte[] colorArray = new byte[CAMERA_WIDTH * CAMERA_HEIGHT * 3];
            colorFrame.CopyTo(colorArray);
            return colorArray;
        }

        /* Run throught the Frames of the VideoStream and apply the Processing function */
        private static void Processing(Pipeline pipeline, Func<Tuple<byte[], ushort[], float[]>, bool> FrameHandler) {
            if (pipeline == null) { return; } // Pipeline is not valide

            // We store the frames as things progress
            FrameSet frames;

            // Run throught the Frames
            while (pipeline.TryWaitForFrames(out frames)) {

                // Extract the Depth and Color from the Frame
                Tuple<byte[], ushort[], float[]> data = Frame(frames);

                // Delegate the Frame Processing to the input function
                FrameHandler(data);

                // Limit the speed of the video
                System.Threading.Thread.Sleep(CAMERA_FRAMERATE);
            }
        }

        // Stream function to Send datas to Unity
        public static void Stream(Pipeline pipeline) {
            Processing(pipeline, StreamFrame);
        }

        // Debug function to Visualise video depth as ascii
        public static void AsciiDepth(Pipeline pipeline) {
            Processing(pipeline, AsciiDepthFrame);
        }

        // Debug function to Visualise video color in command
        public static void AsciiColor(Pipeline pipeline) {
            Processing(pipeline, AsciiColorFrame);
        }

        // Take the Frame as parameter adn Return the Depth and Color datas
        private static Tuple<byte[], ushort[], float[]> Frame(FrameSet frames) {

            Align align = new Align(Intel.RealSense.Stream.Color).DisposeWith(frames);
            Frame aligned = align.Process(frames).DisposeWith(frames);
            FrameSet frameset = aligned.As<FrameSet>().DisposeWith(frames);

            VideoFrame colorFrame = frameset.ColorFrame.DisposeWith(frameset);
            VideoFrame depthFrame = frameset.DepthFrame.DisposeWith(frameset);

            byte[] colorArray = ColorArray(colorFrame);
            ushort[] depthArray = DepthArray(depthFrame);
            float[] verticesArray = VerticeArray(depthFrame);

            return new Tuple<byte[], ushort[], float[]>(colorArray, depthArray, verticesArray);
        }

        // Export the Frame datas to the Api
        private static bool StreamFrame(Tuple<byte[], ushort[], float[]> data) {

            byte[] colors = data.Item1;
            float[] vertices = data.Item3;

            for (int i = 0; i < vertices.Length; i = i + 3) {
                Console.WriteLine("vertice {0} / {1}: \t position [{2},{3},{4}],\t color [{5},{6},{7}]", i / 3, vertices.Length / 3, vertices[i], vertices[i+1], vertices[i+2], colors[i], colors[i + 1], colors[i + 2]);
            }

            return true;
        }

        // Display the frame with Ascii
        private static bool AsciiDepthFrame(Tuple<byte[], ushort[], float[]> data) {

            ushort[] depthArray = data.Item2;

            char[] buffer = new char[(CAMERA_HEIGHT / RES_HEIGHT) * (CAMERA_WIDTH / RES_WIDTH + 1)];
            ushort[] coverage = new ushort[CAMERA_WIDTH / RES_WIDTH];

            int index = 0;
            for (int y = 0; y < CAMERA_HEIGHT; y++) {
                for (int x = 0; x < CAMERA_WIDTH; x++) {
                    ushort depth = depthArray[x + y * CAMERA_WIDTH];
                    if (depth > 0 && depth < RES_DEPTH) { ++coverage[x / RES_WIDTH]; }
                }

                if(y % RES_HEIGHT == RES_HEIGHT-1) { 
                    for (int i = 0; i < coverage.Length; i++)
                    {
                        buffer[index++] = " .:nhBXWW"[coverage[i] / 25];
                        coverage[i] = 0;
                    }
                    buffer[index++] = '\n';
                }
            }

            Console.SetCursorPosition(0, 4);
            Console.WriteLine();
            Console.Write(buffer);

            return true;
        }
        
        // Display the frame in colors
        private static bool AsciiColorFrame(Tuple<byte[], ushort[], float[]> data) {

            byte[] colors = data.Item1;
            ushort[] buffer = new ushort[(CAMERA_HEIGHT / RES_HEIGHT) * (CAMERA_WIDTH / RES_WIDTH + 1) * 3]; // Array of colors [r, g, b]

            Console.SetCursorPosition(0, 4);
            Console.WriteLine();

            for (int y = 0; y < CAMERA_HEIGHT; y++) {
                for (int x = 0; x < CAMERA_WIDTH * 3; x = x + 3) {
                    /* 
                        Problem Here: for me, the formula of the interpolation should be:
                        [x,y] => [round(x / resX) + round(y / resY) * (sizeY / resY) * 3] (+i for the color pos in the [r,g,b])
                        So we have: round(x / resX) for the horizontal position
                        And: round(y / resY) for the vertical position (but we multiply it by the resized horizontal size because 1D array)
                        
                        But it clearly isnt...
                        Return to the line is strange, plus the lasts lines are empty?
                    */
                    for (int i = 0; i < 3; i++) { buffer[(int)(x / RES_WIDTH) + (int)(y / RES_WIDTH) * (CAMERA_WIDTH / RES_WIDTH + 1) + i] += colors[x + y * CAMERA_WIDTH + i]; }
                }
            }

            ushort resolution = RES_WIDTH * RES_HEIGHT;
            for (int i = 0; i < buffer.Length; i = i + 3) {
                byte[] pixel = { (byte)(buffer[i] / resolution), (byte)(buffer[i + 1] / resolution), (byte)(buffer[i + 2] / resolution) };
 
                Console.ForegroundColor = (ConsoleColor)toConsoleColor(pixel);
                Console.Write("█");
                
                if (i % (CAMERA_WIDTH / RES_WIDTH) == (CAMERA_WIDTH / RES_WIDTH) - 1) { Console.WriteLine(); }
            }

            // All pixels without interpolation
            /*
            for (int i = 0; i < colors.Length; i = i + 3) {
                byte[] color = { colors[i], colors[i + 1], colors[i + 2] };
                Console.ForegroundColor = (ConsoleColor)toConsoleColor(color);
                Console.Write("█");
            }
            Console.ReadKey();
            */

            return true;
        }
        
        // Convert an RGB color to and Command color
        private static int toConsoleColor(byte[] color) {
            int index = (color[0] > 128 | color[1] > 128 | color[2] > 128) ? 8 : 0;
            index |= (color[0] > 64) ? 4 : 0;
            index |= (color[1] > 64) ? 2 : 0;
            index |= (color[2] > 64) ? 1 : 0;
            return index;
        }
    }
}
