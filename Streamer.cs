using Intel.RealSense;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace iuF {
    public static class Streamer {

        /* CAMERA PROPERTIES */
        const UInt16 CAMERA_WIDTH = 640;
        const UInt16 CAMERA_HEIGHT = 480;
        const UInt16 CAMERA_FRAMERATE = 30;

        /* RESOLUTIONS (for debug display purpose) */
        const UInt16 RES_WIDTH = 10; // 10px width => 1px width
        const UInt16 RES_HEIGHT = 20; // 20px height => 1px height
        const UInt16 RES_DEPTH = 1500; // maximum depth of display

        private static UInt16[] DepthArray(VideoFrame depthFrame) {
            UInt16[] depthArray = new UInt16[CAMERA_WIDTH * CAMERA_HEIGHT];
            depthFrame.CopyTo(depthArray);
            return depthArray; 
        }
        private static byte[] ColorArray(VideoFrame colorFrame) {
            byte[] colorArray = new byte[CAMERA_WIDTH * CAMERA_HEIGHT * 3];
            colorFrame.CopyTo(colorArray);
            return colorArray;
        }
        public static void Stream(Pipeline pipeline) {
            if (pipeline == null) { return; }

            //int frame = 0;
            FrameSet frames;
            while (pipeline.TryWaitForFrames(out frames)) {
                //Console.WriteLine("Frame number {0}", frame++);
                Frame(frames);
                System.Threading.Thread.Sleep(30);
            }
        }

        private static void Frame(FrameSet frames) {
            Align align = new Align(Intel.RealSense.Stream.Color).DisposeWith(frames);
            Frame aligned = align.Process(frames).DisposeWith(frames);
            FrameSet frameset = aligned.As<FrameSet>().DisposeWith(frames);

            VideoFrame colorFrame = frameset.ColorFrame.DisposeWith(frameset);
            VideoFrame depthFrame = frameset.DepthFrame.DisposeWith(frameset);

            byte[] colorArray = ColorArray(colorFrame);
            UInt16[] depthArray = DepthArray(depthFrame);

            /* Put the Frame alterations / displays functions Here */
            // AsciiDepth(depthArray);
            AsciiColor(colorArray);
            // AsciiDepthColor(colorArray, depthArray);
        }

        private static void AsciiDepth(UInt16[] depthArray) {
            char[] buffer = new char[(CAMERA_HEIGHT / RES_HEIGHT) * (CAMERA_WIDTH / RES_WIDTH + 1)];
            UInt16[] coverage = new UInt16[CAMERA_WIDTH / RES_WIDTH];

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
            //System.Threading.Thread.Sleep(10);
        }
        
        private static void AsciiColor(byte[] colorArray) {
            UInt16[] buffer = new UInt16[(CAMERA_HEIGHT / RES_HEIGHT) * (CAMERA_WIDTH / RES_WIDTH + 1) * 3]; // Array of colors [r, g, b]

            Console.SetCursorPosition(0, 4);
            Console.WriteLine();

            for (int y = 0; y < CAMERA_HEIGHT; y++) {
                for (int x = 0; x < CAMERA_WIDTH * 3; x = x + 3) {
                    for (int i = 0; i < 3; i++) { buffer[(int)(x / RES_WIDTH) + (int)(y / RES_WIDTH) * (CAMERA_WIDTH / RES_WIDTH + 1) + i] += colorArray[x + y * CAMERA_WIDTH + i]; }
                }
            }

            UInt16 resolution = RES_WIDTH * RES_HEIGHT;
            for (int i = 0; i < buffer.Length; i = i + 3) {
                byte[] pixel = { (byte)(buffer[i] / resolution), (byte)(buffer[i + 1] / resolution), (byte)(buffer[i + 2] / resolution) };
                Console.ForegroundColor = (ConsoleColor)toConsoleColor(pixel);
                Console.Write("█");
                if (i % (CAMERA_WIDTH / RES_WIDTH) == (CAMERA_WIDTH / RES_WIDTH) - 1) { Console.WriteLine(); }
            }
        }
        
        private static int toConsoleColor(byte[] color) {
            int index = (color[0] > 128 | color[1] > 128 | color[2] > 128) ? 8 : 0;
            index |= (color[0] > 64) ? 4 : 0;
            index |= (color[1] > 64) ? 2 : 0;
            index |= (color[2] > 64) ? 1 : 0;
            return index;
        }
    }
}
