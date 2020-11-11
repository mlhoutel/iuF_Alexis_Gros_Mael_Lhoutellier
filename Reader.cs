using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace iuF {
    public static class Reader {
        private static Config ConfigFile(this Config config, string filename) {
            config.EnableDeviceFromFile(filename, repeat: false); 
            return config; 
        }
        public static Pipeline FromFile(string filename) {
            if (!File.Exists(filename)) { Console.WriteLine("Error: file {0} does not exists.", filename); return null; }

            Pipeline pipeline = new Pipeline();
            Config config = ConfigFile(new Config(), filename);
            PipelineProfile profile = pipeline.Start(config);

            Device device = profile.Device;
            PlaybackDevice playback = PlaybackDevice.FromDevice(device);

            DateTime begins = DateTime.Now;

            Console.WriteLine("Reading file {0}\nDuration time: {1}\nCamera name: {2}\nSerial number: {3}\nFirmware version: {4}\n", filename, TimeSpan.FromMilliseconds(playback.Duration * 1e-6), device.Info[CameraInfo.Name], device.Info[CameraInfo.SerialNumber], device.Info[CameraInfo.FirmwareVersion]);

            playback.Realtime = false;

            return pipeline;
        }

        private static Config ConfigDevice(this Config config) { 
            config.EnableStream(Intel.RealSense.Stream.Depth);
            config.EnableStream(Intel.RealSense.Stream.Color);
            return config; 
        }
        public static Pipeline FromDevice() {
            Context context = new Context();
            DeviceList devices = context.QueryDevices();

            if (devices.Count == 0) { Console.WriteLine("Error: there is no connected devices."); return null; }
            Device device = devices[0];
            Console.WriteLine("Using device 0\nCamera name: {0}\nSerial number: {1}\nFirmware version: {2}\n", device.Info[CameraInfo.Name], device.Info[CameraInfo.SerialNumber], device.Info[CameraInfo.FirmwareVersion]);

            Pipeline pipeline = new Pipeline();
            Config config = ConfigDevice(new Config());
            PipelineProfile profile = pipeline.Start(config);

            return pipeline;
        }

    }
}
