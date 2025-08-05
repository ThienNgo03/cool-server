namespace Journal.Journeys.Get.ProcessData;

public class Model
{
    public List<ExtendedData> Data { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public bool IsNotesIncluded { get; set; }
}
