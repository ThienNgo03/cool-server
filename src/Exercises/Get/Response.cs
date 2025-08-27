namespace Journal.Exercises.Get;

public class Response : Databases.Journal.Tables.Exercise.Table
{
    public ICollection<Muscle>? Muscles { get; set; }
    
}

public class Muscle : Databases.Journal.Tables.Muscle.Table
{

}