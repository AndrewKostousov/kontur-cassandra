using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEventContent
    {
        public BoxEventContent([NotNull] BoxEntityId entityId, BoxEntityContentType entityContentType, BoxEntityDirection entityDirection)
        {
            if(entityId == null)
                throw new InvalidProgramStateException("entityId is required");
            EntityId = entityId;
            EntityContentType = entityContentType;
            EntityDirection = entityDirection;
        }

        [NotNull]
        public BoxEntityId EntityId { get; private set; }

        public BoxEntityContentType EntityContentType { get; private set; }

        public BoxEntityDirection EntityDirection { get; private set; }

        public override string ToString()
        {
            return string.Format("Type: {0}, EntityId: {1}, EntityContentType: {2}, EntityDirection: {3}", GetType().Name, EntityId, EntityContentType, EntityDirection);
        }
    }
}