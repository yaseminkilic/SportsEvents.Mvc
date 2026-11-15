using System;
using System.Collections.Generic;

namespace SportsEvents.Mvc.Entities;

public partial class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int ParticipantId { get; set; }

    public DateTime RegisteredAt { get; set; }

    public bool IsCancelled { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Participant Participant { get; set; } = null!;
}
