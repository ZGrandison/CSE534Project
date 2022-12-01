using QuicNet.Connections;
using QuicNet.Streams;
using Riptide.Transports.Quic;

namespace QuicNet.Events
{
    public delegate void ClientConnectedEvent(QuicConnection connection, RiptideQuicPeer peer);
    public delegate void StreamOpenedEvent(QuicStream stream, RiptideQuicPeer peer);
    public delegate void StreamDataReceivedEvent(QuicStream stream, byte[] data, RiptideQuicPeer peer);
    public delegate void ConnectionClosedEvent(QuicConnection connection);
}
