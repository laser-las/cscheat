// Overlay.cs
using GameOverlay.Drawing;
using GameOverlay.Windows;
using Swed64;
using static Offsets;

class Overlay
{
    GraphicsWindow window;
    Graphics graphics;
    Font? font;
    Font? fontStor;
    SolidBrush? vit;
    SolidBrush? svart;
    SolidBrush? grön;
    SolidBrush? röd;
    SolidBrush? gul;
    SolidBrush? blå;
    SolidBrush? orange;
    Swed swed;
    IntPtr client;
    readonly Settings settings;
    object lås = new object();
    readonly int screenWidth = GetSystemMetrics(0);
    readonly int screenHeight = GetSystemMetrics(1);

    public bool tbAktiv = false;
    public bool aimAktiv = false;
    const float aimFovRadius = 60f;

    int[][] skelett = new int[][]
    {
        new int[] { 6, 5 },
        new int[] { 5, 4 },
        new int[] { 4, 0 },
        new int[] { 4, 8 },
        new int[] { 8, 9 },
        new int[] { 9, 10 },
        new int[] { 4, 13 },
        new int[] { 13, 14 },
        new int[] { 14, 15 },
        new int[] { 0, 22 },
        new int[] { 22, 23 },
        new int[] { 23, 24 },
        new int[] { 0, 25 },
        new int[] { 25, 26 },
        new int[] { 26, 27 },
    };

    public Overlay(Swed s, IntPtr c, Settings overlaySettings)
    {
        swed = s;
        client = c;
        settings = overlaySettings;
        graphics = new Graphics();
        window = new GraphicsWindow(0, 0, screenWidth, screenHeight, graphics);
        window.IsTopmost = true;
        window.IsVisible = true;
        window.FPS = 144;
    }

    bool WorldToScreen(float[] matrix, float x, float y, float z, out float screenX, out float screenY)
    {
        float w = matrix[12] * x + matrix[13] * y + matrix[14] * z + matrix[15];
        if (w < 0.01f) { screenX = 0; screenY = 0; return false; }
        screenX = (matrix[0] * x + matrix[1] * y + matrix[2] * z + matrix[3]) / w * (screenWidth / 2f) + screenWidth / 2f;
        screenY = -(matrix[4] * x + matrix[5] * y + matrix[6] * z + matrix[7]) / w * (screenHeight / 2f) + screenHeight / 2f;
        return true;
    }

    bool GetBone(IntPtr pawn, int idx, out float ox, out float oy, out float oz)
    {
        ox = oy = oz = 0;
        try
        {
            IntPtr gsn = swed.ReadPointer(pawn, m_pGameSceneNode);
            if (gsn == IntPtr.Zero) return false;
            IntPtr bones = swed.ReadPointer(gsn + m_modelState, m_boneArray);
            if (bones == IntPtr.Zero) return false;
            ox = swed.ReadFloat(bones + idx * 32, 0);
            oy = swed.ReadFloat(bones + idx * 32, 4);
            oz = swed.ReadFloat(bones + idx * 32, 8);
            if (Math.Abs(ox) > 100000 || Math.Abs(oy) > 100000) return false;
            return true;
        }
        catch { return false; }
    }

    float BeröknaAvstånd(float x, float y, float z)
    {
        try
        {
            IntPtr lp = swed.ReadPointer(client, dwLocalPlayerPawn);
            if (lp == IntPtr.Zero) return 0;
            var lpos = swed.ReadVec(lp, m_vOldOrigin);
            float dx = x - lpos.X, dy = y - lpos.Y, dz = z - lpos.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz) / 100f;
        }
        catch { return 0; }
    }

    void Text(Font f, SolidBrush fg, SolidBrush bg, float x, float y, string t)
    {
        graphics.DrawText(f, bg, x - 1, y, t);
        graphics.DrawText(f, bg, x + 1, y, t);
        graphics.DrawText(f, bg, x, y - 1, t);
        graphics.DrawText(f, bg, x, y + 1, t);
        graphics.DrawText(f, fg, x, y, t);
    }

    void DrawHpBar(float x1, float y2, float x2, int hp)
    {
        float w = x2 - x1;
        float hw = w * (hp / 100f);
        float top = y2 + 2, bot = y2 + 6;
        graphics.FillRectangle(svart!, x1 - 1, top - 1, x2 + 1, bot + 1);
        SolidBrush c = hp > 60 ? grön! : hp > 30 ? gul! : röd!;
        graphics.FillRectangle(c, x1, top, x1 + hw, bot);
    }

    void DrawBox(float feetX, float feetY, float headY, float höjd, SolidBrush färg)
    {
        float b = höjd / 2.5f;
        float l = feetX - b / 2, r = feetX + b / 2;
        graphics.DrawRectangle(svart!, l - 1, headY - 1, r + 1, feetY + 1, 2);
        graphics.DrawRectangle(färg, l, headY, r, feetY, 1);
    }

    void DrawSkelett(float[] vm, IntPtr pawn, SolidBrush färg)
    {
        foreach (var par in skelett)
        {
            try
            {
                if (!GetBone(pawn, par[0], out float ax, out float ay, out float az)) continue;
                if (!GetBone(pawn, par[1], out float bx, out float by, out float bz)) continue;
                if (!WorldToScreen(vm, ax, ay, az, out float sax, out float say)) continue;
                if (!WorldToScreen(vm, bx, by, bz, out float sbx, out float sby)) continue;
                if (sax < 0 || sax > screenWidth || say < 0 || say > screenHeight) continue;
                if (sbx < 0 || sbx > screenWidth || sby < 0 || sby > screenHeight) continue;
                graphics.DrawLine(svart!, sax - 1, say, sbx - 1, sby, 2);
                graphics.DrawLine(svart!, sax + 1, say, sbx + 1, sby, 2);
                graphics.DrawLine(färg, sax, say, sbx, sby, 1);
            }
            catch { }
        }
    }

    public void Start(List<Info> lista)
    {
        window.SetupGraphics += (sender, e) =>
        {
            font     = graphics.CreateFont("Fixedsys", 10);
            fontStor = graphics.CreateFont("Fixedsys", 12);
            vit      = graphics.CreateSolidBrush(255, 255, 255);
            svart    = graphics.CreateSolidBrush(0, 0, 0);
            grön     = graphics.CreateSolidBrush(0, 255, 0);
            röd      = graphics.CreateSolidBrush(255, 50, 50);
            gul      = graphics.CreateSolidBrush(255, 220, 0);
            blå      = graphics.CreateSolidBrush(50, 150, 255);
            orange   = graphics.CreateSolidBrush(255, 165, 0);
        };

        window.DrawGraphics += (sender, e) =>
        {
            graphics.ClearScene();
            if (font == null || fontStor == null || vit == null || svart == null ||
                grön == null || röd == null || gul == null || blå == null || orange == null)
                return;

            float[] matrix = swed.ReadMatrix(client + dwViewMatrix);

            graphics.DrawCircle(aimAktiv ? gul : vit, screenWidth / 2f, screenHeight / 2f, aimFovRadius, 1);

            Text(font, tbAktiv  ? grön : röd, svart, 10, 10, tbAktiv  ? "TB: PÅ"  : "TB: AV");
            Text(font, aimAktiv ? grön : röd, svart, 10, 24, aimAktiv ? "AIM: PÅ" : "AIM: AV");

            lock (lås)
            {
                for (int i = 0; i < lista.Count; i++)
                {
                    if (!lista[i].IsAlive) continue;

                    float x = lista[i].posX, y = lista[i].posY, z = lista[i].posZ;

                    if (!WorldToScreen(matrix, x, y, z, out float feetX, out float feetY)) continue;

                    float headX = feetX, headY;
                    if (GetBone(lista[i].pawn, 6, out float hbx, out float hby, out float hbz) &&
                        WorldToScreen(matrix, hbx, hby, hbz, out float hsx, out float hsy))
                    {
                        headX = hsx;
                        headY = hsy;
                    }
                    else
                    {
                        if (!WorldToScreen(matrix, x, y, z + 70, out _, out float hy2)) continue;
                        headY = hy2;
                    }

                    if (GetBone(lista[i].pawn, 24, out float lfx, out float lfy, out float lfz) &&
                        GetBone(lista[i].pawn, 27, out float rfx, out float rfy, out float rfz) &&
                        WorldToScreen(matrix, lfx, lfy, lfz, out float lsx, out float lsy) &&
                        WorldToScreen(matrix, rfx, rfy, rfz, out float rsx, out float rsy))
                    {
                        feetX = (lsx + rsx) / 2f;
                        feetY = Math.Max(lsy, rsy);
                    }

                    float höjd = feetY - headY;
                    if (höjd < 5 || höjd > screenHeight) continue;
                    if (feetX < 0 || feetX > screenWidth || feetY < 0 || feetY > screenHeight) continue;
                    if (headX < 0 || headX > screenWidth || headY < 0 || headY > screenHeight) continue;

                    float bredd = höjd / 2;
                    SolidBrush lagFärg = lista[i].Team == 2 ? gul : blå;
                    float dist = BeröknaAvstånd(x, y, z);
                    if (dist > settings.MaxDistanceMeters) continue;

                    if (settings.ShowBox)
                        DrawBox(feetX, feetY, headY, höjd, lagFärg);
                    if (settings.ShowSkeleton)
                        DrawSkelett(matrix, lista[i].pawn, lagFärg);

                    float radie = Math.Max(3f, höjd / 14f);
                    graphics.DrawCircle(svart!, headX, headY, radie + 1, 2);
                    graphics.DrawCircle(lagFärg, headX, headY, radie, 1);

                    int hp = Math.Clamp(lista[i].hp, 0, 100);
                    if (settings.ShowHealth)
                        DrawHpBar(feetX - bredd / 2, feetY, feetX + bredd / 2, hp);

                    string hpText = $"{hp}hp";
                    if (settings.ShowHealth)
                        Text(font, grön, svart, headX - hpText.Length * 3f, headY - 14, hpText);

                    string namn = lista[i].Name ?? "";
                    if (settings.ShowNames && namn.Length > 0)
                        Text(fontStor, vit, svart, headX - namn.Length * 3.5f, headY - 26, namn);

                    string vapen = lista[i].vapen ?? "";
                    if (settings.ShowWeapon && vapen.Length > 0)
                        Text(font, orange, svart, feetX - (vapen.Length + 5) * 3f, feetY + 10, $"{vapen} [{lista[i].ammo}]");

                    if (settings.ShowDistance && dist > 0)
                        Text(font, vit, svart, feetX - 10, feetY + 22, $"{dist:F0}m");
                }
            }
        };

        window.Create();
        window.Join();
    }

    public object GetLås() { return lås; }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern int GetSystemMetrics(int index);
}