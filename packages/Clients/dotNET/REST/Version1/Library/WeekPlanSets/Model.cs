using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.WeekPlanSets;

public class Model
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid InsertedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    public Guid UpdatedBy { get; set; }
    public Guid WeekPlanId { get; set; }
    public int Value { get; set; }
}
