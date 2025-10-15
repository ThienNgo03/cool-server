namespace BFF.Exercises.All;

public class Response 
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; }
    public string Title { get; set; }
    public string SubTitle { get; set; }
    public string Description { get; set; }
    public string Badge { get; set; }
    public string PercentageCompleteString { get; set; }
    public double PercentageComplete { get; set; }
    public string BadgeTextColor { get; set; }
    public string BadgeBackgroundColor { get; set; }
}