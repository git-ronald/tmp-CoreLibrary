﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLibrary.SchedulerService
{
    public class SchedulerTaskList : List<Func<CancellationToken, Task>>
    {
    }
}
