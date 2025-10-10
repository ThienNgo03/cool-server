namespace Journal.Exercises.SuperSearch;

public class Response : Model
{
    public ICollection<Muscle>? Muscles { get; set; }

}

public class Muscle : Muscles.Model
{

}
