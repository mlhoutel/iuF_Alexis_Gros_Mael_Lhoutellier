using System;
using Intel.RealSense;

namespace iuF {
    class Program {
        static void Main(string[] args) {
            //Reader.FromFile(@"D:\Documents\IMT\iuF\test.bag");
            Streamer.Stream(Reader.FromFile(@"D:\Documents\IMT\iuF\outdoors.bag"));
            //Reader.FromDevice();
        }
    }
}
