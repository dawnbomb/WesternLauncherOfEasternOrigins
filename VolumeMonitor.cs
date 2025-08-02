using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace WesternLauncherOfEasternOrigins
{
    public static class VolumeMonitor
    {
        private static HashSet<int> knownSessionPIDs = new();

        public static void StartVolumeMonitoring()
        {
            Task.Run(async () =>
            {
                // Capture initial audio sessions (before game launch)
                knownSessionPIDs = GetCurrentSessionPIDs();

                var deviceEnumerator = new MMDeviceEnumerator();
                var device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalSeconds < 20)
                {
                    var sessions = device.AudioSessionManager.Sessions;

                    for (int i = 0; i < sessions.Count; i++)
                    {
                        try
                        {
                            var session = sessions[i];
                            var session2 = session as IAudioSessionControl2;
                            if (session2 == null) continue;

                            session2.GetProcessId(out uint pid);
                            int processId = (int)pid;

                            if (knownSessionPIDs.Contains(processId))
                                continue;

                            float volume = session.SimpleAudioVolume.Volume;
                            bool isMuted = session.SimpleAudioVolume.Mute;

                            Debug.WriteLine($"New audio PID={processId}, Volume={volume}, Muted={isMuted}");

                            if (!isMuted && volume >= 0.99f)
                            {
                                OpenVolumeMixer();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error checking session: {ex.Message}");
                        }
                    }

                    await Task.Delay(500);
                }
            });
        }

        private static HashSet<int> GetCurrentSessionPIDs()
        {
            var result = new HashSet<int>();
            var enumerator = new MMDeviceEnumerator();
            var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var sessions = device.AudioSessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                try
                {
                    var session2 = sessions[i] as IAudioSessionControl2;
                    if (session2 != null)
                    {
                        session2.GetProcessId(out uint pid);
                        result.Add((int)pid);
                    }
                }
                catch { /* Ignore failures */ }
            }

            return result;
        }

        private static void OpenVolumeMixer()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "sndvol",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open volume mixer: {ex.Message}");
            }
        }
    }
}