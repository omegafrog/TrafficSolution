using TrafficForm.Domain;
using TrafficForm.Port;

namespace TrafficForm.Adapter
{
    public sealed class VdsTrafficSnapshotRefresher : IAsyncDisposable, IDisposable, IVdsTrafficSnapshotRefresherPort
    {
        private readonly VdsTrafficSnapshotStore _store;
        private readonly IVdsTrafficSnapshotSourcePort _source;
        private readonly TimeSpan _interval;
        private readonly object _lifecycleLock = new object();
        private CancellationTokenSource? _stoppingCts;
        private Task? _loopTask;
        private PeriodicTimer? _timer;
        private int _isRefreshing;
        private bool _isStopping;

        public VdsTrafficSnapshotRefresher(
            VdsTrafficSnapshotStore store,
            IVdsTrafficSnapshotSourcePort source,
            TimeSpan? interval = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _interval = interval ?? TimeSpan.FromMinutes(2);
        }

        public void Start()
        {
            lock (_lifecycleLock)
            {
                if (_isStopping)
                {
                    return;
                }

                if (_loopTask is { IsCompleted: false })
                {
                    return;
                }

                _stoppingCts = new CancellationTokenSource();
                _timer = new PeriodicTimer(_interval);
                _loopTask = RunLoopAsync(_stoppingCts.Token);
            }
        }

        public async Task StopAsync()
        {
            CancellationTokenSource? stoppingCts;
            Task? loopTask;

            lock (_lifecycleLock)
            {
                stoppingCts = _stoppingCts;
                loopTask = _loopTask;

                if (stoppingCts == null || loopTask == null)
                {
                    return;
                }

                _isStopping = true;
            }

            stoppingCts.Cancel();

            try
            {
                await loopTask.ConfigureAwait(false);
            }
            finally
            {
                lock (_lifecycleLock)
                {
                    if (_loopTask == loopTask)
                    {
                        _timer?.Dispose();
                        _timer = null;
                        _loopTask = null;
                        _stoppingCts = null;
                        _isStopping = false;
                    }
                }

                stoppingCts.Dispose();
            }
        }

        public async Task RefreshOnceAsync(CancellationToken ct)
        {
            if (Interlocked.CompareExchange(ref _isRefreshing, 1, 0) != 0)
            {
                return;
            }

            try
            {
                IReadOnlyDictionary<string, VdsTrafficObservation> data =
                    await _source.FetchAsync(ct).ConfigureAwait(false);

                DateTimeOffset now = DateTimeOffset.UtcNow;
                VdsTrafficSnapshot current = _store.GetCurrent();
                // Version increments on every refresh attempt to remain monotonic.
                long nextVersion = current.Version + 1;
                VdsTrafficSnapshot next = new VdsTrafficSnapshot(
                    Guid.NewGuid(),
                    nextVersion,
                    data,
                    lastSuccessUtc: now,
                    lastAttemptUtc: now,
                    lastError: null);

                _store.Swap(next);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                VdsTrafficSnapshot current = _store.GetCurrent();
                long nextVersion = current.Version + 1;
                VdsTrafficSnapshot failure = new VdsTrafficSnapshot(
                    current.SnapshotId,
                    nextVersion,
                    current.ByVdsId,
                    lastSuccessUtc: current.LastSuccessUtc,
                    lastAttemptUtc: now,
                    lastError: ex.ToString());

                _store.Swap(failure);
            }
            finally
            {
                Interlocked.Exchange(ref _isRefreshing, 0);
            }
        }

        public void Dispose()
        {
            StopAsync().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private async Task RunLoopAsync(CancellationToken stoppingToken)
        {
            try
            {
                await TryRefreshAsync(stoppingToken).ConfigureAwait(false);

                while (await _timer!.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
                {
                    await TryRefreshAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        private async Task TryRefreshAsync(CancellationToken stoppingToken)
        {
            try
            {
                await RefreshOnceAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
        }
    }
}
