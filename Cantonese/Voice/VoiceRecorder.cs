using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;

namespace Cantonese.Voice
{
    class VoiceRecorder
    {
        private MediaCapture _mediaCapture;
        private InMemoryRandomAccessStream _memoryBuffer = new InMemoryRandomAccessStream();
        public bool IsRecording { get; set; }

        public async void Record()
        {
            if (IsRecording)
            {
                throw new InvalidOperationException("Recording already in progress!");
            }
            //await Initialize();
            //await DeleteExistingFile();
            MediaCaptureInitializationSettings settings =
              new MediaCaptureInitializationSettings
              {
                  StreamingCaptureMode = StreamingCaptureMode.Audio
              };
            _mediaCapture = new MediaCapture();
            await _mediaCapture.InitializeAsync(settings);
            await _mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto), _memoryBuffer);
            IsRecording = true;
        }

        public async Task<InMemoryRandomAccessStream> StopRecording()
        {
            await _mediaCapture.StopRecordAsync();
            IsRecording = false;
            return _memoryBuffer;
        }

    }
}
