using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxIdentifier
    {
        public BoxIdentifier([NotNull] string boxId, [NotNull] string partyId)
        {
            if(string.IsNullOrEmpty(boxId))
                throw new InvalidProgramStateException("boxId is empty");
            if(string.IsNullOrEmpty(partyId))
                throw new InvalidProgramStateException(string.Format("partyId is empty for boxId: {0}", boxId));
            BoxId = boxId;
            PartyId = partyId;
        }

        [NotNull]
        public string BoxId { get; private set; }

        [NotNull]
        public string PartyId { get; private set; }

        public override string ToString()
        {
            return string.Format("BoxId: {0}, PartyId: {1}", BoxId, PartyId);
        }
    }
}