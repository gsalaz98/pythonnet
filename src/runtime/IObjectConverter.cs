using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Python.Runtime
{
    public interface IObjectConverter
    {
        IntPtr ToPython(object instance);
        IntPtr ToManaged(object instance);
    }
}
