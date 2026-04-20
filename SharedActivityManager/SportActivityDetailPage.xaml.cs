using SharedActivityManager.Factories;
using SharedActivityManager.Models;
using SharedActivityManager.Data;

namespace SharedActivityManager
{
    public partial class SportActivityDetailPage : ContentPage
    {
        private Activity _activity;
        private SportActivityCreator _creator;
        private System.Timers.Timer _timer;
        private bool _isDisposing = false;

        public SportActivityDetailPage(Activity activity)
        {
            InitializeComponent();
            _activity = activity;
            _creator = new SportActivityCreator();

            TitleLabel.Text = activity.Title;
            DescriptionLabel.Text = activity.Desc;

            UpdateDisplay();
            StartTimerUpdates();
        }

        private void UpdateDisplay()
        {
            var data = _creator.GetSportData(_activity);

            TimerLabel.Text = _creator.GetFormattedTime(_activity);
            ProgressBar.Progress = data.GetProgressPercentage() / 100;
            ProgressLabel.Text = $"{data.GetProgressPercentage():F0}%";
            CaloriesLabel.Text = data.CaloriesBurned.ToString();
            RepsLabel.Text = data.Repetitions.ToString();
            DistanceLabel.Text = $"{data.DistanceKm:F1} km";

            bool isRunning = data.IsTimerRunning;
            StartButton.IsVisible = !isRunning;
            PauseButton.IsVisible = isRunning;
        }

        private void StartTimerUpdates()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += async (sender, e) =>
            {
                if (_isDisposing) return;

                await _creator.TickTimerAsync(_activity);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (!_isDisposing)
                    {
                        UpdateDisplay();

                        if (_activity.IsCompleted)
                        {
                            _timer.Stop();
                            DisplayAlert("Complete!", "Workout completed!", "OK");
                        }
                    }
                });
            };
            _timer.Start();
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            _creator.StartTimer(_activity);
            UpdateDisplay();
        }

        private void OnPauseClicked(object sender, EventArgs e)
        {
            _creator.PauseTimer(_activity);
            UpdateDisplay();
        }

        private void OnStopClicked(object sender, EventArgs e)
        {
            _creator.StopTimer(_activity);
            UpdateDisplay();
        }

        private void OnAddRepClicked(object sender, EventArgs e)
        {
            _creator.AddRepetition(_activity);
            UpdateDisplay();
        }

        private void OnAddDistanceClicked(object sender, EventArgs e)
        {
            if (double.TryParse(DistanceEntry.Text, out double km))
            {
                _creator.AddDistance(_activity, km);
                DistanceEntry.Text = string.Empty;
                UpdateDisplay();
            }
        }

        private async void OnCompleteClicked(object sender, EventArgs e)
        {
            _isDisposing = true;
            _timer?.Stop();
            _activity.IsCompleted = true;

            var database = new ActivityDataBase();
            await database.SaveActivityAsync(_activity);

            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isDisposing = true;
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}