using GrainInterfaces;
using Microsoft.Extensions.Logging;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Grains;

public class HelloGrain : Grain, IHello, IDisposable
{
    private readonly ILogger _logger;
    private readonly ISubject<long> _ticksSubj;
    private readonly IObservable<long> _ticks;
    private readonly IDisposable _ticksSubscription;

    public HelloGrain(ILogger<HelloGrain> logger)
    {
        _logger = logger;
        _ticksSubj = new Subject<long>();
        _ticks = _ticksSubj.AsObservable();
        _ticksSubscription = _ticks.Subscribe(x => _logger.LogInformation($"Tick received {x}"));
    }
    public override Task OnActivateAsync(CancellationToken token)
    {
        _logger.LogInformation("OnActivateAsync");
        return Task.CompletedTask;
    }

    ValueTask<string> IHello.SayHello(string greeting)
    {
        _logger.LogInformation(
            "SayHello message received: greeting = '{Greeting}'", greeting);

        return ValueTask.FromResult(
            $"""
            Client said: '{greeting}', so HelloGrain says: Hello!
            """);
    }

    Task IHello.DoTick(long tick)
    {
        _ticksSubj.OnNext(tick);
        return Task.CompletedTask;
    }

    ValueTask<string> IHello.ApplyDot(int ticks)
    {
        _logger.LogInformation($"ApplyDot message received: number of ticks to process = {ticks}");

        var rxScheduler = new TaskPoolScheduler(new TaskFactory(TaskScheduler.Current));

        // NOTE: be sure to dispose any observables before the grain deactivates.
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(ticks)
            .SubscribeOn(rxScheduler)
            .ObserveOn(rxScheduler)
            .Subscribe(x => _ticksSubj.OnNext(x));

        return ValueTask.FromResult($"Applying DoT with {ticks} ticks.");
    }

    public void Dispose()
    {
        _ticksSubscription.Dispose();
    }
}