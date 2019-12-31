using System.Collections.Generic;
using Xamarin.Forms;

namespace XamapenCvCam.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

#if true
            // https://rksoftware.hatenablog.com/entry/2018/01/02/025156
            // UI の構築。 Plugin の使い方とは直接関係ない
            var sl = (StackLayout)(this.Content = new StackLayout() { VerticalOptions = LayoutOptions.Fill });
            var button1 = new Button() { Text = "Take Photo" };
            var image1 = new Image() { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
            sl.Children.Add(button1);
            sl.Children.Add(image1);

            // ボタンクリックイベント。 この中身が Plugin の使い方
            button1.Clicked += async (sender, e) =>
            {
                // Plugin の初期化。おまじない
                await Plugin.Media.CrossMedia.Current.Initialize();

                // カメラが使用可能で、写真が撮影可能かを判定。
                if (!Plugin.Media.CrossMedia.Current.IsCameraAvailable || !Plugin.Media.CrossMedia.Current.IsTakePhotoSupported)
                {
                    // カメラが使用不可または写真が撮影不可の場合、終了
                    return;
                }

                // カメラが起動し写真を撮影する。撮影した写真はストレージに保存され、ファイルの情報が return される
                var file = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    // ストレージに保存するファイル情報
                    // すでに同名ファイルがある場合は、temp_1.jpg などの様に連番がつけられ名前の衝突が回避される
                    Directory = "TempPhotos",
                    Name = "temp.jpg"
                });

                // 端末のカメラ機能でユーザーが「キャンセル」した場合は、file が null となる
                if (file == null)
                    return;

                // 今回独自に試してみた部分
                // ストレージに保存された社員ファイルの中身をメモリに読み込み、ファイルは削除してしまう
                var bytes = new Queue<byte>();
                using (var s = file.GetStream())
                {
                    var length = s.Length;
                    int b;
                    while ((b = s.ReadByte()) != -1)
                        bytes.Enqueue((byte)b);
                }
                System.IO.File.Delete(file.Path);
                file.Dispose();

                // 写真を画面上の image 要素に表示する
                image1.Source = ImageSource.FromStream(() =>
                {
                    // 元のサンプルはファイルから読み込んでいたが、
                    // 今回独自にメモリからの表示を試している
                    return new System.IO.MemoryStream(bytes.ToArray());
                });
            };
#endif
        }
    }
}