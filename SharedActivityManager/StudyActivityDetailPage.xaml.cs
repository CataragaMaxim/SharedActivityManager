using CommunityToolkit.Maui.Views;
using SharedActivityManager.Factories;
using SharedActivityManager.Models;
using SharedActivityManager.Data;

namespace SharedActivityManager
{
    public partial class StudyActivityDetailPage : ContentPage
    {
        private Activity _activity;
        private StudyActivityCreator _creator;
        private StudyActivityData _data;
        private int _currentQuizIndex = 0;
        private bool _isVideoProgressUpdating = false;

        public StudyActivityDetailPage(Activity activity)
        {
            InitializeComponent();
            _activity = activity;
            _creator = new StudyActivityCreator();
            _data = _creator.GetStudyData(activity);

            TitleLabel.Text = activity.Title;
            DescriptionLabel.Text = activity.Desc;
            UrlEntry.Text = _data.VideoUrl;
            NotesEditor.Text = _data.Notes;

            VideoProgressBar.Progress = _data.VideoProgress / 100;
            VideoProgressLabel.Text = $"{_data.VideoProgress:F0}%";

            if (_data.QuizQuestions.Any())
            {
                QuizSection.IsVisible = true;
                LoadCurrentQuizQuestion();
            }

            // Subscribe la evenimentele video
            VideoPlayer.MediaEnded += OnVideoEnded;
            VideoPlayer.PositionChanged += OnVideoPositionChanged;
        }

        private void OnLoadVideoClicked(object sender, EventArgs e)
        {
            var url = UrlEntry.Text;
            if (!string.IsNullOrEmpty(url))
            {
                var embedUrl = _creator.GetYouTubeEmbedUrl(url);
                VideoPlayer.Source = MediaSource.FromUri(embedUrl);
                _creator.SetVideoUrl(_activity, url);
            }
        }

        private async void OnVideoPositionChanged(object sender, CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
        {
            // Evită actualizări multiple simultane
            if (_isVideoProgressUpdating) return;
            _isVideoProgressUpdating = true;

            try
            {
                // Verifică dacă Duration este disponibil
                var duration = VideoPlayer.Duration;
                if (duration.TotalSeconds > 0)
                {
                    var progress = e.Position.TotalSeconds / duration.TotalSeconds * 100;
                    await _creator.UpdateVideoProgressAsync(_activity, progress);

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        VideoProgressBar.Progress = progress / 100;
                        VideoProgressLabel.Text = $"{progress:F0}%";
                    });
                }
            }
            finally
            {
                _isVideoProgressUpdating = false;
            }
        }

        private async void OnVideoEnded(object sender, EventArgs e)
        {
            await _creator.UpdateVideoProgressAsync(_activity, 100);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlert("Complete!", "Video completed!", "OK");
                VideoProgressBar.Progress = 1;
                VideoProgressLabel.Text = "100%";
            });
        }

        private async void OnSaveNoteClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NotesEditor.Text))
            {
                _creator.AddNote(_activity, NotesEditor.Text);
                await DisplayAlert("Success", "Note saved!", "OK");
                NotesEditor.Text = string.Empty;
            }
        }

        private void LoadCurrentQuizQuestion()
        {
            if (_currentQuizIndex < _data.QuizQuestions.Count)
            {
                var q = _data.QuizQuestions[_currentQuizIndex];
                QuizQuestionLabel.Text = q.Question;
                QuizOptionsView.ItemsSource = q.Options;
                QuizScoreLabel.Text = $"Score: {_data.CurrentQuizScore}/{_data.QuizQuestions.Count}";
            }
            else
            {
                QuizSection.IsVisible = false;
                var percent = _creator.GetQuizScorePercentage(_activity);
                DisplayAlert("Quiz Complete!", $"Final score: {_data.CurrentQuizScore}/{_data.QuizQuestions.Count} ({percent:F0}%)", "OK");
            }
        }

        private async void OnSubmitAnswerClicked(object sender, EventArgs e)
        {
            if (QuizOptionsView.SelectedItem is string selectedOption)
            {
                var selectedIndex = _data.QuizQuestions[_currentQuizIndex].Options.IndexOf(selectedOption);
                var newScore = _creator.SubmitQuizAnswer(_activity, _currentQuizIndex, selectedIndex);
                _data = _creator.GetStudyData(_activity);

                _currentQuizIndex++;
                LoadCurrentQuizQuestion();
            }
            else
            {
                await DisplayAlert("Warning", "Please select an answer", "OK");
            }
        }

        private async void OnCompleteClicked(object sender, EventArgs e)
        {
            _activity.IsCompleted = true;

            var database = new ActivityDataBase();
            await database.SaveActivityAsync(_activity);

            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Cleanup
            if (VideoPlayer != null)
            {
                VideoPlayer.MediaEnded -= OnVideoEnded;
                VideoPlayer.PositionChanged -= OnVideoPositionChanged;
                VideoPlayer.Source = null;
            }
        }
    }
}