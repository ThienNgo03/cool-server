namespace Journal.Journeys.Get;

public class Storage
{
    public List<Data> Data { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public bool IsNotesIncluded { get; set; }
}