﻿using System;
using System.Collections.Generic;

namespace ProCoder.Models;

public partial class Theme
{
    public int ThemeId { get; set; }

    public string ThemeName { get; set; } = null!;

    public int ThemeOrder { get; set; }

    public virtual ICollection<Problem> Problems { get; set; } = new List<Problem>();
}
