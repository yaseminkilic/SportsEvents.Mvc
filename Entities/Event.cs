using System;
using System.Collections.Generic;

namespace SportsEvents.Mvc.Entities;

public partial class Event
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartAt { get; set; }

    public int Capacity { get; set; }

    public int RegisteredCount { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
