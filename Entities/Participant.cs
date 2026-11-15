using System;
using System.Collections.Generic;

namespace SportsEvents.Mvc.Entities;

public partial class Participant
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
