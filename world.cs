using Swed64;
using static Offsets;

class WorldToScreen
{
    public static bool Convert(Swed swed, IntPtr client, float x, float y, float z, out float screenX, out float screenY)
    {
        float[] matrix = swed.ReadMatrix(client + dwViewMatrix);

        screenX = matrix[0] * x + matrix[1] * y + matrix[2] * z + matrix[3];
        screenY = matrix[4] * x + matrix[5] * y + matrix[6] * z + matrix[7];
        float w = matrix[12] * x + matrix[13] * y + matrix[14] * z + matrix[15];

        if (w < 0.01f)
        {
            screenX = 0;
            screenY = 0;
            return false;
        }

        int screenWidth = GetSystemMetrics(0);
        int screenHeight = GetSystemMetrics(1);
        screenX = (screenX / w) * (screenWidth / 2f) + (screenWidth / 2f);
        screenY = -(screenY / w) * (screenHeight / 2f) + (screenHeight / 2f);
        return true;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern int GetSystemMetrics(int index);
}