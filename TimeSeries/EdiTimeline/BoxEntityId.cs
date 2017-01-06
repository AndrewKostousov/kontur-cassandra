using System;
using Commons;
using JetBrains.Annotations;

namespace EdiTimeline
{
    public class BoxEntityId
    {
        private BoxEntityId(Guid? entityId, Guid? transportMessageId, Guid? connectorMessageId)
        {
            EntityId = entityId;
            TransportMessageId = transportMessageId;
            ConnectorMessageId = connectorMessageId;
        }

        [NotNull]
        public static BoxEntityId ForEntity([NotNull] string entityId)
        {
            Guid entityIdGuid;
            if (!Guid.TryParse(entityId, out entityIdGuid))
                throw new InvalidProgramStateException(string.Format("Invalid entityId: {0}", entityId));
            return ForEntity(entityIdGuid);
        }

        [NotNull]
        public static BoxEntityId ForEntity(Guid entityId)
        {
            return new BoxEntityId(entityId, null, null);
        }

        [NotNull]
        public static BoxEntityId ForTransportMessage([NotNull] string transportMessageId)
        {
            Guid transportMessageIdGuid;
            if (!Guid.TryParse(transportMessageId, out transportMessageIdGuid))
                throw new InvalidProgramStateException(string.Format("Invalid transportMessageId: {0}", transportMessageId));
            return ForTransportMessage(transportMessageIdGuid);
        }

        [NotNull]
        public static BoxEntityId ForTransportMessage(Guid transportMessageId)
        {
            return new BoxEntityId(null, transportMessageId, null);
        }

        [NotNull]
        public static BoxEntityId ForConnectorMessage([NotNull] string connectorMessageId)
        {
            Guid connectorMessageIdGuid;
            if (!Guid.TryParse(connectorMessageId, out connectorMessageIdGuid))
                throw new InvalidProgramStateException(string.Format("Invalid connectorMessageId: {0}", connectorMessageId));
            return ForConnectorMessage(connectorMessageIdGuid);
        }

        [NotNull]
        public static BoxEntityId ForConnectorMessage(Guid connectorMessageId)
        {
            return new BoxEntityId(null, null, connectorMessageId);
        }

        public Guid? EntityId { get; private set; }
        public Guid? TransportMessageId { get; private set; }
        public Guid? ConnectorMessageId { get; private set; }

        public bool IsForEntity()
        {
            return EntityId.HasValue;
        }

        public bool IsForTransportMessage()
        {
            return TransportMessageId.HasValue;
        }

        public bool IsForConnectorMessage()
        {
            return ConnectorMessageId.HasValue;
        }

        public Guid GetEntityId()
        {
            if (!EntityId.HasValue)
                throw new InvalidProgramStateException(string.Format("EntityId is not set for: {0}", this));
            return EntityId.Value;
        }

        public Guid GetTransportMessageId()
        {
            if (!TransportMessageId.HasValue)
                throw new InvalidProgramStateException(string.Format("TransportMessageId is not set for: {0}", this));
            return TransportMessageId.Value;
        }

        public Guid GetConnectorMessageId()
        {
            if (!ConnectorMessageId.HasValue)
                throw new InvalidProgramStateException(string.Format("ConnectorMessageId is not set for: {0}", this));
            return ConnectorMessageId.Value;
        }

        public override string ToString()
        {
            if (EntityId.HasValue && !TransportMessageId.HasValue && !ConnectorMessageId.HasValue)
                return string.Format("EntityId: {0}", EntityId);
            if (!EntityId.HasValue && TransportMessageId.HasValue && !ConnectorMessageId.HasValue)
                return string.Format("TransportMessageId: {0}", TransportMessageId);
            if (!EntityId.HasValue && !TransportMessageId.HasValue && ConnectorMessageId.HasValue)
                return string.Format("ConnectorMessageId: {0}", ConnectorMessageId);
            return string.Format("EntityId: {0}, TransportMessageId: {1}, ConnectorMessageId: {2}", EntityId, TransportMessageId, ConnectorMessageId);
        }
    }
}