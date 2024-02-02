using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Windows.Media.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Markup;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Video_Annotation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Annotator : Page
    {
        public bool isPlaying = false;
        private DispatcherTimer timerVideoTime;
        private Notation notation = null;
        private readonly List<Notation> notations = new List<Notation>();
        private readonly Dictionary<string, Color> labels;

        public Annotator()
        {
            this.InitializeComponent();
            var config = new AppConfig();
            labels = new Dictionary<string, Color>();
            foreach (var label in config.Labels)
            {
                var color =  (Color)XamlBindingHelper.ConvertValue(typeof(Color), label.Color);
                labels.Add(label.Name, color);

                var txtLabel = new TextBlock
                {
                    Text = label.Name,
                    Foreground = new SolidColorBrush(color)
                };
                NotationCombo.Items.Add(txtLabel);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Playback.SetPlaybackSource((MediaSource)e.Parameter);
            Debug.WriteLine("OnNavigatedTo");
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                if (isPlaying)
                {
                    Playback.Pause();
                    isPlaying = false;
                }
                else
                {
                    Playback.Play();
                    isPlaying = true;
                }
            }
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Page_GotFocus");
        }

        private void Page_LostFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Page_LostFocus");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                Playback.Pause();
                isPlaying = false;
                CmdPlay.Content = "Play";
            }
            else
            {
                Playback.Play();
                isPlaying = true;
                CmdPlay.Content = "Pause";
            }
        }

        private void Searcher_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Playback.Position = TimeSpan.FromSeconds(Searcher.Value);
        }

        private void Playback_MediaOpened(object sender, RoutedEventArgs e)
        {
            Searcher.Maximum = Playback.NaturalDuration.TimeSpan.TotalSeconds;

            timerVideoTime = new DispatcherTimer();
            timerVideoTime.Interval = TimeSpan.FromSeconds(1);
            timerVideoTime.Tick += TimerVideoTime_Tick;
            timerVideoTime.Start();
        }

        private void TimerVideoTime_Tick(object sender, object e)
        {
            Searcher.Value = Playback.Position.TotalSeconds;
            if (!(notation is null))
            {
                notation.EndTime = Playback.Position.TotalSeconds;
                endTimeTxt.Text = notation.EndTime.ToString();
            }

            if (Searcher.Value >= Searcher.Maximum) {
                if (!(notation is null))
                {
                    notations.Add(notation);
                    notation = null;
                }
                SaveCmd.IsEnabled = true;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NotationCmd.IsEnabled = true;
        }

        private void NotationCmd_Click(object sender, RoutedEventArgs e)
        {
            if (NotationCombo.IsEnabled)
            {
                NotationCombo.IsEnabled = false;
                NotationCmd.Content = "Stop";
                notation = new Notation
                {
                    Label = ((TextBlock)NotationCombo.SelectedItem).Text,
                    StartTime = Playback.Position.TotalSeconds
                };
                startTimeTxt.Text = notation.StartTime.ToString();
            }
            else
            {
                NotationCombo.IsEnabled = true;
                NotationCmd.Content = "Start";
                notation.EndTime = Playback.Position.TotalSeconds;
                notations.Add(notation);
                DisplayAnnotation(notation);
                notation = null;
            }
        }

        private void DisplayAnnotation(Notation note)
        {
            var rect = new Rectangle();
            rect.Name = $"period_{note.GetHashCode()}";
            var x_scale = annotation_map.ActualWidth / Playback.NaturalDuration.TimeSpan.TotalSeconds;
            rect.SetValue(Canvas.LeftProperty, x_scale * note.StartTime);
            rect.SetValue(Canvas.TopProperty, 0);
            rect.Width = x_scale * (note.EndTime - note.StartTime);
            rect.Height = (int)annotation_map.Height;
            rect.Fill = new SolidColorBrush() { Color = labels[note.Label], Opacity = .5f };
            annotation_map.Children.Add(rect);

            var label = new Grid();
            label.Name = $"label_{note.GetHashCode()}";
            label.Width = 120;
            label.Height = 40;
            label.HorizontalAlignment = HorizontalAlignment.Left;
            label.Margin = new Thickness { Left = x_scale * note.StartTime };
            var lblRect = new Rectangle { Height = 30, RadiusX = 10, RadiusY = 10, Fill = new SolidColorBrush { Color = Colors.Yellow }, Stroke = new SolidColorBrush { Color = Colors.DarkGray } };
            Canvas.SetZIndex(label, 0);
            lblRect.PointerMoved += (s, e) =>
            {
                foreach (var item in annotation_row.Children)
                    Canvas.SetZIndex(item, 0);
                Canvas.SetZIndex(label, 1);
            };
            label.Children.Add(lblRect);
            var lblTxt = new TextBlock { Margin = new Thickness(5, 10, 5, 5), Text = note.Label, Foreground = new SolidColorBrush(Colors.Black) };
            lblTxt.PointerMoved += (s, e) =>
            {
                foreach (var item in annotation_row.Children)
                    Canvas.SetZIndex(item, 0);
                Canvas.SetZIndex(label, 1);
            };
            label.Children.Add(lblTxt);
            var cmdDel = new Button { HorizontalAlignment = HorizontalAlignment.Right, Content = "🗑️", Margin = new Thickness(0, 0, 5, 0) };
            cmdDel.Click += (s, e) => RemoveAnnotation(note);
            cmdDel.PointerMoved += (s, e) =>
            {
                foreach (var item in annotation_row.Children)
                    Canvas.SetZIndex(item, 0);
                Canvas.SetZIndex(label, 1);
            };
            label.Children.Add(cmdDel);
            annotation_row.Children.Add(label);
        }

        private void RemoveAnnotation(Notation note)
        {
            notations.Remove(note);
            var an = (Rectangle)annotation_map.FindName($"period_{note.GetHashCode()}");
            annotation_map.Children.Remove(an);
            var label = (Grid)annotation_row.FindName($"label_{note.GetHashCode()}");
            annotation_row.Children.Remove(label);
        }

        private async void SaveCmd_Click(object sender, RoutedEventArgs e)
        {
            var csvFileDialog = new FileSavePicker();
            csvFileDialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            csvFileDialog.SuggestedFileName = "test";
            csvFileDialog.FileTypeChoices.Add(".csv", new List<string> { ".csv" });
            StorageFile csvFile = await csvFileDialog.PickSaveFileAsync();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false
            };
            using (var csv = new CsvWriter(new StreamWriter(await csvFile.OpenStreamForWriteAsync()), config))
            {
                csv.WriteRecords(notations);
            }
        }
    }
}
