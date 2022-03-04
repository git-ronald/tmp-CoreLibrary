namespace CoreLibrary.PeerInterface
{
    public class PeerRegistrationInfo
    {
        public Guid PeerId { get; set; }
        public string PeerName { get; set; } = string.Empty;
        public Guid PeerNodeId { get; set; }
    }
}
