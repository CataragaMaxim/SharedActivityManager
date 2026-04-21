namespace SharedActivityManager.Models.Decorators
{
    /// <summary>
    /// Decorator pentru tracking GPS (specific pentru activități sport)
    /// </summary>
    public class GpsTrackingDecorator : ActivityDecorator
    {
        private double _distance;
        private double _duration;

        public GpsTrackingDecorator(IActivityExtra inner) : base(inner)
        {
        }

        public override string Name => "GPS Tracking";
        public override bool IsEnabled => true;

        public override string GetDescription()
        {
            string trackingInfo = _distance > 0 ? $" ({_distance:F1} km, {_duration:F0} min)" : "";
            return $"{_inner.GetDescription()} + 🗺️ GPS Tracking{trackingInfo}";
        }

        public override int GetExtraCost()
        {
            return _inner.GetExtraCost() + 5; // 5 minute pentru configurare GPS
        }

        public override string GetIcon()
        {
            return "🗺️";
        }

        public void UpdateTracking(double distanceKm, double durationMinutes)
        {
            _distance = distanceKm;
            _duration = durationMinutes;
        }

        public override async Task ExecuteAsync(Activity activity)
        {
            await _inner.ExecuteAsync(activity);

            // Simulare salvare date GPS
            System.Diagnostics.Debug.WriteLine($"🗺️ GPS Data saved: {_distance:F1} km in {_duration:F0} minutes");
        }
    }
}