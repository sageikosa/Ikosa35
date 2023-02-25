using Uzi.Packaging;

namespace Uzi.Core
{
    public class CoreProcessPart : PartWrapper<CoreProcess>
    {
        public CoreProcessPart(CoreProcess process)
            : base(process, process.SourceName())
        {
        }
    }
}
