public class TimerService
{
    private readonly object _lock = new();
    private int _remainingSeconds = 120;
    private DateTime? _endTime = null;
    private bool _running = false;

    public string Start()
    {
        lock (_lock)
        {
            if (!_running)
            {
                _running = true;
                _endTime = DateTime.UtcNow.AddSeconds(_remainingSeconds);
            }
            return "Timer started";
        }
    }

    public object Status()
    {
        lock (_lock)
        {
            if (!_running || _endTime == null)
                return new { remainingSeconds = _remainingSeconds, message = "Timer not running" };

            var secondsLeft = (int)(_endTime.Value - DateTime.UtcNow).TotalSeconds;
            if (secondsLeft <= 0)
            {
                _running = false;
                _remainingSeconds = 0;
                return new { remainingSeconds = 0, message = "Time's up!" };
            }
            return new { remainingSeconds = secondsLeft, message = "Timer running" };
        }
    }

    public string Reset()
    {
        lock (_lock)
        {
            _running = false;
            _remainingSeconds = 120;
            _endTime = null;
            return "Timer reset";
        }
    }
}
