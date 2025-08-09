namespace Test.Databases.Journal.Tables.WeekPlan
{
    public class Table
    {
        public Guid Id { get; set; }

        public Guid WorkoutId { get; set; }

        public string DateOfWeek { get; set; } // làm sao để nó chỉ lấy thứ?

        public DateTime Time { get; set; } 

        public int Rep { get; set; } // làm sao để nó chỉ lấy một trong hai Rep hoặc HoldingTime

        public TimeSpan HoldingTime { get; set; }

        public int Set { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}

public enum DateOfWeek
{
    Monday , Tuesday, Wednesday, Thursday , Friday, Saturday, Sunday
}
