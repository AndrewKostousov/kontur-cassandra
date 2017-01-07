using System;

namespace EdiTimeline.Tests
{
    public class DummyEventContent : BoxEventContent
    {
        public DummyEventContent(Guid entityId)
            : base(BoxEntityId.ForEntity(entityId), BoxEntityContentType.Unknown, BoxEntityDirection.Undefined)
        {
        }
    }
}