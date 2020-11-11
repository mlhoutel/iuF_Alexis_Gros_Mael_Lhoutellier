using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Text;

namespace iuF {
    public static class Streamer {

        const UInt16 CAMERA_WIDTH = 640;
        const UInt16 CAMERA_HEIGHT = 480;
        const UInt16 CAMERA_FRAMERATE = 30;

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

            int frame = 0;
            FrameSet frames;
            while (pipeline.TryWaitForFrames(out frames)) {
                Console.WriteLine("Frame number {0}", frame++);
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
            AsciiDepth(depthArray);
        }

        private static void AsciiDepth(UInt16[] depthArray) {
            for (int y = 0; y < CAMERA_HEIGHT; y++) {
                for (int x = 0; x < CAMERA_WIDTH; x++) {
                }
            }
            //System.Threading.Thread.Sleep(1000);
        }
    }
}
