namespace Journal.Exercises.Get;

public class Response : Model
{
    public ICollection<Muscle>? Muscles { get; set; }
    
}

public class Muscle : Muscles.Model
{

}