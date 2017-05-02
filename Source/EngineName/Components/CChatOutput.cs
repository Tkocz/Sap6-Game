using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineName.Core;

namespace EngineName.Components
{
    public class CChatOutput: EcsComponent
    {
        public string Message { get; set; }
    }
}
