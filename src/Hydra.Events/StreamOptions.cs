using System;

namespace Hydra.Events
{
    public class StreamOptions
    {
        public Boolean CreateContainer { get; set; }

        public Boolean CreateBlob { get; set; }

        public Boolean AppendDelimeter { get; set; }

        public StreamOptions()
        {
            CreateContainer = true;
            CreateBlob = true;
            AppendDelimeter = true;
        }
    }
}
