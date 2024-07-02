using System;

namespace RoboticArmSystem.Core.Utils
{
    public class GuidGenerator
    {
        public static Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
