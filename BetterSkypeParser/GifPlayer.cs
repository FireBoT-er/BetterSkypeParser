using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BetterSkypeParser
{
    public class GifPlayer : IDisposable
    {
        private readonly Avalonia.Controls.Image? ImageControl;
        private readonly string? GifUri;
        private readonly List<Bitmap> Frames = new();
        private readonly List<int> FrameDelays = new();
        private DispatcherTimer? Timer;
        private int CurrentFrame;
        private readonly string EmptyInstanceErrorInfo = "Empty instance is used only to indicate the presence of a GIF";

        public GifPlayer() { }

        public GifPlayer(Avalonia.Controls.Image imageControl, string gifUri)
        {
            ImageControl = imageControl;
            GifUri = gifUri;
        }

        public async Task Play()
        {
            if (ImageControl == null)
            {
                throw new ArgumentException(EmptyInstanceErrorInfo);
            }

            using var httpClient = new HttpClient();
            using var stream = await httpClient.GetStreamAsync(GifUri);
            using var gif = await Image.LoadAsync(stream);

            Frames.Clear();
            FrameDelays.Clear();

            foreach (var frame in gif.Frames)
            {
                int delay = frame.Metadata.GetGifMetadata().FrameDelay;
                FrameDelays.Add(delay*10);

                using var tempImage = new Image<Rgba32>(frame.Width, frame.Height);
                tempImage.Frames.InsertFrame(0, frame);

                using var memoryStream = new MemoryStream();
                await tempImage.SaveAsPngAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                Frames.Add(new Bitmap(memoryStream));
            }

            CurrentFrame = 0;

            Timer = new();
            Timer.Tick += (_, _) =>
            {
                ImageControl.Source = Frames[CurrentFrame];
                Timer.Interval = TimeSpan.FromMilliseconds(FrameDelays[CurrentFrame]);

                CurrentFrame = (CurrentFrame + 1) % Frames.Count;
            };

            Timer.Interval = TimeSpan.FromMilliseconds(FrameDelays[0]);
            Timer.Start();
        }

        public void Continue() => Timer?.Start();

        public void Stop() => Timer?.Stop();

        public void Dispose()
        {
            if (ImageControl == null)
            {
                throw new ArgumentException(EmptyInstanceErrorInfo);
            }

            Timer?.Stop();
            ImageControl.Source = null;

            foreach (var frame in Frames)
            {
                frame.Dispose();
            }

            Frames.Clear();
            FrameDelays.Clear();

            GC.SuppressFinalize(this);
        }
    }
}