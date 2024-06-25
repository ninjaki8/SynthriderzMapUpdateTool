using SharpAdbClient;
using System.Net;

namespace SynthriderzMapUpdateTool
{
    public class DeviceManager
    {
        private AdbClient AdbClient { get; set; } = new();
        private static DeviceData? QuestDevice { get; set; }

        public string StartAdbServer()
        {
            var server = new AdbServer();
            var adbPath = CreateAdbResources();
            server.StartServer(adbPath, restartServerIfNewer: false);
            var serverStatus = server.GetStatus().ToString();

            return serverStatus;
        }

        private string CreateAdbResources()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appData, Globals.ApplicationFolder);
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            var files = new Dictionary<string, byte[]?>() {
                {"adb.exe", Properties.Resources.adb },
                {"AdbWinApi.dll", Properties.Resources.AdbWinApi},
                {"AdbWinUsbApi.dll", Properties.Resources.AdbWinUsbApi}
            };

            foreach (var file in files)
            {
                string path = Path.Combine(appFolder, file.Key);
                if (!File.Exists(path) && file.Value != null)
                {
                    File.WriteAllBytes(path, file.Value);
                }
            }

            return Path.Combine(appFolder, "adb.exe");
        }

        public List<string> GetAdbDevices()
        {
            var deviceModelList = new List<string>();
            var devices = AdbClient.GetDevices();
            devices?.ForEach(x => deviceModelList.Add(x.Model));

            return deviceModelList;
        }

        public bool GetDeviceByModel(string model)
        {
            try
            {
                var devices = AdbClient.GetDevices();
                var questDevice = devices.Where(x => x.Model == model).First();
                if (questDevice == null)
                {
                    return false;
                }

                QuestDevice = questDevice;

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> GetSynthFolder()
        {
            try
            {
                var excludeDir = new List<string> { "proc", "dev", "sys", "system", "system_ext", "vendor", "vendor_dlkm", "odm", "odm_dlkm" };
                string command = $"find / -type d";
                excludeDir.ForEach(dir => command += $" -not \\( -path \"*/{dir}/*\" -prune \\)");
                var receiver = new ConsoleOutputReceiver();
                await AdbClient.ExecuteRemoteCommandAsync(command, QuestDevice, receiver, CancellationToken.None);

                using StringReader reader = new(receiver.ToString());
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(Globals.SynthRidersCustomSongs))
                    {
                        return line;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public async Task<List<string>> GetSynthFiles(string path)
        {
            try
            {
                var beatmaps = new List<string>();

                var command = $"ls {path} | grep .synth";
                var receiver = new ConsoleOutputReceiver();
                await AdbClient.ExecuteRemoteCommandAsync(command, QuestDevice, receiver, CancellationToken.None);

                using StringReader reader = new(receiver.ToString());
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    beatmaps.Add(line);
                }

                return beatmaps;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return [];
            }
        }

        public static void DeviceFileUpload(Stream file, string fileName, string destinationFolder)
        {
            try
            {
                string filePath = $"{destinationFolder}/{fileName}";

                using var service = new SyncService(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)), QuestDevice);

                service.Push(file, filePath, 660, DateTime.Now, null, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
