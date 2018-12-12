using System;

namespace Xabaril.Store
{
    public class Feature
    {
        public static Feature EnabledFeature(string name) => new Feature { Name = name, CreatedOn = DateTime.UtcNow, Enabled = true };
        public static Feature DisabledFeature(string name) => new Feature { Name = name, CreatedOn = DateTime.UtcNow};

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsDisabled()
        {
            return !Enabled;
        }
    }
}
