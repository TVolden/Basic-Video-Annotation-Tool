using System;
using Windows.Media.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Video_Annotation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //Test.Source = new Uri(@"c:\Users\vasev\OneDrive - ITU\data\AffectDynamics-pilot\20231205-Subject1\video1006826136.mp4");
            //Test.Play();
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker MediaContent = new FileOpenPicker();
            MediaContent.ViewMode = PickerViewMode.List;
            MediaContent.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            //add the media file ectension for filtering  
            MediaContent.FileTypeFilter.Add(".wmv");
            MediaContent.FileTypeFilter.Add(".wma");
            MediaContent.FileTypeFilter.Add(".mp4");
            MediaContent.FileTypeFilter.Add(".mts");
            StorageFile openmedia = await MediaContent.PickSingleFileAsync();
            //Test.SetPlaybackSource(MediaSource.CreateFromStorageFile(openmedia));
            Frame.Navigate(typeof(Annotator), MediaSource.CreateFromStorageFile(openmedia));
        }
    }
}
