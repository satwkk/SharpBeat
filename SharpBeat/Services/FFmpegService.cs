
using System.Diagnostics;

namespace SharpBeat.Services {

    public class AudioConverter : IDisposable {

        readonly Process _handle;

        public Process Handle => _handle;

        public AudioConverter() {
            _handle = new Process() {
                StartInfo = new ProcessStartInfo {
                    FileName = "ffmpeg",
                    Arguments = "-i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1 -loglevel quiet -nostats",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            _handle.Start();
        }

        public void Dispose() {
            _handle.Close();
        }

        public async Task<Memory<byte>> ConvertToPCM(Stream audioStream) {
            using (var memoryStream = new MemoryStream()) {
                var inputTask = Task.Run(async () => {
                    await audioStream.CopyToAsync(_handle.StandardInput.BaseStream);
                    _handle.StandardInput.BaseStream.Close();
                });

                var outputTask = Task.Run(async () => {
                    await _handle.StandardOutput.BaseStream.CopyToAsync(memoryStream);
                });

                await Task.WhenAll(inputTask, outputTask);

                return new Memory<byte>(memoryStream.ToArray());
            }
        }

        public async Task FlushAll() {
            await _handle.StandardInput.BaseStream.FlushAsync();
            await _handle.StandardOutput.BaseStream.FlushAsync();
            await _handle.StandardError.BaseStream.FlushAsync();
        }
    }

}