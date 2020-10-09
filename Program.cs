using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intel.RealSense;

namespace iuF_Alexis_Gros_Mael_Lhoutellier
{
    public static class Program
    {
        const int CAMERA_WIDTH = 640;
        const int CAMERA_HEIGHT = 480;

        const int FPS = 30;

        // renvoie un tableau avec les composantes r,g,b du pixel demandé
        static byte[] getColor(int posX, int posY, byte[] colorArray)
        {
            int index = (posX + (posY * CAMERA_WIDTH)) * 3;
            return new byte[] { colorArray[index], colorArray[index+1], colorArray[index+2] };
        }

        static UInt16 getDepth(int posX, int posY, UInt16[] depthArray)
        {
            int index = posX + (posY * CAMERA_WIDTH);
            return depthArray[index];
        }

        static void runCycle(Pipeline pipe)
        {
            using (var frames = pipe.WaitForFrames())
            {
                Align align = new Align(Stream.Color).DisposeWith(frames);
                Frame aligned = align.Process(frames).DisposeWith(frames);
                FrameSet alignedframeset = aligned.As<FrameSet>().DisposeWith(frames);
                var colorFrame = alignedframeset.ColorFrame.DisposeWith(alignedframeset);
                var depthFrame = alignedframeset.DepthFrame.DisposeWith(alignedframeset);

                var colorArray = new byte[CAMERA_WIDTH * CAMERA_HEIGHT * 3];
                colorFrame.CopyTo(colorArray);
                var depthArray = new UInt16[CAMERA_WIDTH * CAMERA_HEIGHT];
                depthFrame.CopyTo(depthArray);

                showPixelInfos(CAMERA_WIDTH/2, CAMERA_HEIGHT/2, colorArray, depthArray);
            }
        }

        static void runFromDevice()
        {
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, CAMERA_WIDTH, CAMERA_HEIGHT, Format.Z16, FPS);
            cfg.EnableStream(Stream.Color, CAMERA_WIDTH, CAMERA_HEIGHT, Format.Rgb8, FPS);

            var pipe = new Pipeline();
            pipe.Start(cfg);
            Console.WriteLine("Reading from Device ");

            while (true)
            {
                runCycle(pipe);
            }
        }

        static Config FromFile(this Config cfg, string file) { cfg.EnableDeviceFromFile(file, repeat: false); return cfg; }

        static void runFromFile(String fileName)
        {
            using (var pipe = new Pipeline())
            using (var cfg = FromFile(new Config(), fileName))
            using (var pp = pipe.Start(cfg))
            using (var dev = pp.Device)
            using (var playback = PlaybackDevice.FromDevice(dev))
            {
                Console.WriteLine("Reading from : " + playback.FileName);
                playback.Realtime = false;

                while (playback.Status != PlaybackStatus.Stopped)
                {
                    runCycle(pipe);
                }
            }
        }

        static void Main(string[] args)
        {
            runFromFile("test.bag");
            Console.Clear();
            runFromDevice();
        }

        // Combine les tableaux de couleur et de profondeur pour un envoi plus facile à travers le réseau
        // Juste pour le fun, servira probablement à rien puisqu'il faut faire un squellette d'abord
        static byte[] combineColorDepthArrays(byte [] colorArray, UInt16[] depthArray)
        {
            var colorDepthArray = new byte[CAMERA_WIDTH * CAMERA_HEIGHT * 5];
            for (int i = 0; i < CAMERA_WIDTH * CAMERA_HEIGHT; i++)
            {
                colorDepthArray[i * 4] = colorArray[i * 3];
                colorDepthArray[i * 4 + 1] = colorArray[i * 3 + 1];
                colorDepthArray[i * 4 + 2] = colorArray[i * 3 + 2];
                var depthBytes = BitConverter.GetBytes(depthArray[i]);
                colorDepthArray[i * 4 + 3] = depthBytes[0];
                colorDepthArray[i * 4 + 4] = depthBytes[1];
            }
            return colorDepthArray;
        }

        static void showColor(byte[] color)
        {
            Console.WriteLine("color: [" + color[0].ToString() + "," + color[1].ToString() + "," + color[2].ToString() + "]");
        }

        static void showDepth(UInt16 depth)
        {
            Console.WriteLine("depth: " + depth.ToString());
        }

        static void quickClear(int nbLine)
        {
            Console.SetCursorPosition(0, 4);
            for (int i = 0; i < nbLine; i++)
            {
                Console.WriteLine("                               ");
            }
            Console.SetCursorPosition(0, 4);
        }

        static void showPixelInfos(int posX, int posY, byte[] colorArray, UInt16[] depthArray)
        {
            var colorDepth = getPixelInfos(posX, posY, colorArray, depthArray);
            quickClear(3);
            Console.WriteLine("--- ("+posX+","+posY+") ---");
            showColor(colorDepth.Item1);
            showDepth(colorDepth.Item2);
        }
        static Tuple<byte[],UInt16> getPixelInfos(int posX, int posY, byte[] colorArray, UInt16[] depthArray)
        {
            var color = getColor(posX, posY, colorArray);
            var depth = getDepth(CAMERA_WIDTH / 2, CAMERA_HEIGHT / 2, depthArray);

            return new Tuple<byte[], ushort>(color, depth);
        }
    }
}
