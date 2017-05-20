using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thengill.Core;

namespace Thengill.Components
{
    public class CSyncObject : EcsComponent
    {
        public bool Owner = true;
        public string fileName = "";
    }
}
