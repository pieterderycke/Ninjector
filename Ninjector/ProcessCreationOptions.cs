using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ninjector
{
    [Flags]
    public enum ProcessCreationOptions
    {
        NormalPriorityClass = 0x00000020,
        CreateSuspended = 0x00000004,
    }
}
