﻿using System;
using System.Collections.Generic;

namespace DeviceManager.Lib.Models;

public partial class gitRole
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
