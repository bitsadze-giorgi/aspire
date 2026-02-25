using Orleans.Runtime;
using Sample.GrainInterfaces;

namespace Sample.Grains;

public class CounterGrain(
    [PersistentState("counter", "Default")] IPersistentState<CounterState> state
) : Grain, ICounterGrain
{
    public async Task<int> Increment()
    {
        state.State.Count++;
        await state.WriteStateAsync();
        return state.State.Count;
    }

    public Task<int> GetCount() => Task.FromResult(state.State.Count);
}

[GenerateSerializer]
public class CounterState
{
    [Id(0)]
    public int Count { get; set; }
}
