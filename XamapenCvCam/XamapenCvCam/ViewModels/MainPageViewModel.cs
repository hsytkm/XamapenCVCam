using Plugin.Media;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamapenCvCam.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
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

        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Main Page";

            IsCameraAvailable = CrossMedia.Current.IsCameraAvailable;
            IsTakePhotoSupported = CrossMedia.Current.IsTakePhotoSupported;
            IsTakeVideoSupported = CrossMedia.Current.IsTakeVideoSupported;

            TakePhotoCommand = new DelegateCommand(
                async () => await TakePhotoAsync(),
                () => IsCameraAvailable && IsTakePhotoSupported);

            TakeVideoCommand = new DelegateCommand(
                async () => await TakeVideoAsync(),
                () => IsCameraAvailable && IsTakeVideoSupported);
        }

        private async Task TakePhotoAsync()
        {
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
            var file = await CrossMedia.Current.TakeVideoAsync(
                new Plugin.Media.Abstractions.StoreVideoOptions
                {
                    DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Front,
                    AllowCropping = false,
                    DesiredLength = TimeSpan.FromSeconds(3),
                });
            if (file == null) return;

            TakeImage = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                file.Dispose();
                return stream;
            });
        }

    }
}
