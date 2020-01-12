using Android.Graphics;
using Plugin.Media;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XamapenCvCam.Models;
using Xamarin.Forms;

namespace XamapenCvCam.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService _pageDialogService;

        public bool IsCameraAvailable { get; }
        public bool IsTakePhotoSupported { get; }
        public bool IsTakeVideoSupported { get; }

        public ImageSource TakeImage
        {
            get => _takeImage;
            private set => SetProperty(ref _takeImage, value);
        }
        private ImageSource _takeImage;

        public DelegateCommand TakePhotoCommand { get; }
        public DelegateCommand TakeVideoCommand { get; }

        public MainPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _pageDialogService = pageDialogService;
            Title = "Main Page";

            CrossMedia.Current.Initialize();
            IsCameraAvailable = CrossMedia.Current.IsCameraAvailable;
            IsTakePhotoSupported = CrossMedia.Current.IsTakePhotoSupported;
            IsTakeVideoSupported = CrossMedia.Current.IsTakeVideoSupported;

            TakePhotoCommand = new DelegateCommand(
                async () => await TakeNegaPosiPhotoAsync(),
                () => IsCameraAvailable && IsTakePhotoSupported);

            TakeVideoCommand = new DelegateCommand(
                async () => await TakeVideoAsync(),
                () => IsCameraAvailable && IsTakeVideoSupported);
        }

        private async Task TakeNegaPosiPhotoAsync()
        {
            await CrossMedia.Current.Initialize();

            using var file = await CrossMedia.Current.TakePhotoAsync(
                new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    AllowCropping = false,
                });
            if (file == null) return;

#if true
            var controller = new ImageController(file.GetStream());

            var data = controller.GetAverageG();

#else
            using var stream = file.GetStream();
            var bitmap = BitmapFactory.DecodeStream(stream);

            ulong sumb = 0, sumg = 0, sumr = 0;
            var width = bitmap.Width;
            var height = bitmap.Height;

            var pixels = new int[width * height];
            bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);

            for (var y = 0; y < height * width; y += width)
            {
                for (var x = 0; x < width; x++)
                {
                    var p = pixels[y + x];
                    sumb += (ulong)((p >> 16) & 0xff);
                    sumg += (ulong)((p >> 8) & 0xff);
                    sumr += (ulong)(p & 0xff);
                }
            }

            double aveb = sumb / (double)(width * height);
            double aveg = sumg / (double)(width * height);
            double aver = sumr / (double)(width * height);
            bitmap.Recycle();
            bitmap.Dispose();
#endif

            TakeImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });

            await Application.Current.MainPage.DisplayAlert("Title", $"Data={data:f2}", "OK");
        }

        private async Task TakePhotoAsync()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakePhotoAsync(
                new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    AllowCropping = false,
                });
            if (file == null) return;

            TakeImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

        private async Task TakeVideoAsync()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakeVideoAsync(
                new Plugin.Media.Abstractions.StoreVideoOptions
                {
                    Directory = "DefaultVideos",
                    Name = "video.mp4",
                    DesiredLength = TimeSpan.FromSeconds(3),
                });
            if (file == null) return;

            await _pageDialogService.DisplayAlertAsync("Video Recorded", "Location: " + file.Path, "OK");

            // ◆Video(mp4)の先頭フレームを表示したいけど実装が分からない…
            TakeImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

    }
}
