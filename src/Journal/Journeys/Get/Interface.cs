namespace Journal.Journeys.Get;

public interface Interface
{
    Task<Storage> GetStorage(Parameters parameters);
    Task<ProcessData.Model> ProcessStorage(Storage storage);
    Task<Result> CreateResult(ProcessData.Model processData);
}
