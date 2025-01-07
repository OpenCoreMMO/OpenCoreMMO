namespace NeoServer.Scripts.LuaJIT.Structs;

public record struct UserDataStruct(int Index, IntPtr Ptr, int ReferenceIndex)
{
    public int Index { get; set; } = Index;
    public IntPtr Ptr { get; set; } = Ptr;
    public int ReferenceIndex { get; set; } = ReferenceIndex;
}