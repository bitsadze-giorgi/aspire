namespace Sample.GrainInterfaces;

public interface ICounterGrain : IGrainWithStringKey
{
    Task<int> Increment();
    Task<int> GetCount();
}
