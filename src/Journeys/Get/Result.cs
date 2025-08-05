namespace Journal.Journeys.Get;

public class Result
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public bool IsNotesIncluded { get; set; }
    public List<ProcessData.ExtendedData> Data { get; set; }
}
