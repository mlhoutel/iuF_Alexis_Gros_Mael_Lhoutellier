using System;
using Intel.RealSense;

namespace iuF {
    class Program {
        static void Main(string[] args) {

            /* We use the Reader object to get the datas from the File or from the Device */
            //Pipeline fromdevice = Reader.FromDevice();
            Pipeline fromfile = Reader.FromFile(@"D:\Documents\IMT\iuF\test.bag");

            /* We then use the Streamer object to extract the frames and transmit the datas */

            /* Display raw datas*/
            //Streamer.DisplayPixels(fromfile); 
            Streamer.DisplayPoints(fromfile);

            /* Visualisation functions */
            //Streamer.AsciiDepth(fromfile); 
            // Streamer.AsciiColor(fromfile);

            /* Send raw datas */
            // Streamer.Stream(fromfile); // Function to complete in the Part 2 (display points to the display device)
        }
    }
}
