using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTerra.DataModel;
using UnityEngine;

namespace OpenTerra.Unity
{
    public static class Converter
    {
        public static Vector3 FromCartesian3(Cartesian3 c)
        {
            return new Vector3((float)c.x, (float)c.y, (float)c.z);
        }
    }
}
