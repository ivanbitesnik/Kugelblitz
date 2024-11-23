namespace GodotSteam;

public struct FriendMessage
{
    public int Ret { get; set; }
    public string Text { get; set; }
    public SteamStatic.ChatEntryType Type { get; set; }
}