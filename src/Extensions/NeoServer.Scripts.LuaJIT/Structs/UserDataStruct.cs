namespace NeoServer.Scripts.LuaJIT.Structs;

public struct UserDataStruct(int index, IntPtr ptr, int referenceIndex)
{
    public int Index { get; set; } = index;
    public IntPtr Ptr { get; set; } = ptr;
    public int ReferenceIndex { get; set; } = referenceIndex;
}