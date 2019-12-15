using XLua;
public class GLuaEnvironment
{
    private LuaEnv luaEnv;

    public GLuaEnvironment()
    {

    }

    private void Init()
    {
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(LuaLoader);
    }

    private byte[] LuaLoader(ref string filePath)
    {
        return new byte[10];

#if UNITY_EDITOR

        // AddLuaSubfix(ref filePath);

        // filePath = context.fileSystem.SearchFile(filePath);

        // if (string.IsNullOrEmpty(filePath))
        //     return null;

        // return FileUtils.ReadBytesFromFile(filePath);

#else

        //             AddLuaSubfix(ref filePath);
        // #if !UNITY_EDITOR
        //             if(Is64bit())
        //             {
        //                 filePath = "x64/" + filePath;
        //             }
        //             else
        //             {
        //                 filePath = "x86/" + filePath;
        //             }
        // #endif

        //             filePath = context.fileSystem.SearchFile(filePath);

        //             if (string.IsNullOrEmpty(filePath))
        //                 return null;

        //             return FileUtils.ReadBytesFromFile(filePath);



        /*
        byte[] fileContent = FileUtils.ReadBytesFromFile(filePath);

        int size = Marshal.SizeOf(fileContent[0]) * fileContent.Length;
        IntPtr pnt = Marshal.AllocHGlobal(size);
        try
        {
            // Copy the array to unmanaged memory.
            Marshal.Copy(fileContent, 0, pnt, fileContent.Length);
            IntPtr decodeData;
            int decodeLen = XCoreInterface.xDecodeData(pnt, fileContent.Length, out decodeData);
            byte[] luaData = new byte[decodeLen];
            Marshal.Copy(decodeData, luaData, 0, decodeLen);
            XCoreInterface.xFreeCharPtr(decodeData);
            return luaData;
        }
        finally
        {
            // Free the unmanaged memory.
            Marshal.FreeHGlobal(pnt);
        }
        */
#endif
    }

    private void AddLuaSubfix(ref string filePath)
    {
        filePath = filePath.Replace('.', '/');
        if (!filePath.EndsWith(".lua"))
        {
            filePath += ".lua";
        }
    }

}