namespace CoreLibrary.PeerInterface
{
    public class PeerRegistrationInfo
    {
        public PeerRegistrationInfo(Guid peerId, string peerName)
        {
            PeerId = peerId;
            PeerName = peerName;
        }

        public Guid PeerId { get; set; }
        public string PeerName { get; set; } = string.Empty;
    }
}
