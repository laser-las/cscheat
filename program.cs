using Swed64;
using System.Diagnostics;
using static Offsets;

Process[] processes = Process.GetProcessesByName("cs2");
if (processes.Length == 0)
{
    Console.WriteLine("öppna cs först");
}
else
{
    Swed swed = new Swed("cs2");
    IntPtr client = swed.GetModuleBase("client.dll");
    List<Info> lista = new List<Info>();
    Settings settings = new Settings();
    Overlay overlay = new Overlay(swed, client, settings);
    Radar radar = new Radar();

    Thread overlayThread = new Thread(() => overlay.Start(lista));
    overlayThread.Start();

    const float halfW   = 960f;
    const float halfH   = 540f;
    const float centerX = 960f;
    const float centerY = 540f;
    const float trigFov = 20f;
    const float aimFov  = 60f;

    // ── TRIGGERBOT [X = toggle] ──────────────────────────────────
    Thread triggerbotThread = new Thread(() =>
    {
        bool xHeld = false;
        while (true)
        {
            bool xKey = (GetAsyncKeyState(0x58) & 0x8000) != 0;
            if (xKey && !xHeld)
            {
                overlay.tbAktiv = !overlay.tbAktiv;
                Console.Clear();
                Console.ForegroundColor = overlay.tbAktiv ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(overlay.tbAktiv ? "TRIGGERBOT: PÅ" : "TRIGGERBOT: AV");
                Console.ResetColor();
            }
            xHeld = xKey;

            if (!overlay.tbAktiv) { Thread.Sleep(1); continue; }

            try
            {
                float[] matrix = swed.ReadMatrix(client + dwViewMatrix);
                bool skaSkjuta = false;

                lock (overlay.GetLås())
                {
                    foreach (var p in lista)
                    {
                        if (!p.IsAlive) continue;

                        IntPtr gsn = swed.ReadPointer(p.pawn, m_pGameSceneNode);
                        if (gsn == IntPtr.Zero) continue;
                        IntPtr bones = swed.ReadPointer(gsn + m_modelState, m_boneArray);
                        if (bones == IntPtr.Zero) continue;

                        float hx = swed.ReadFloat(bones + 6 * 32, 0);
                        float hy = swed.ReadFloat(bones + 6 * 32, 4);
                        float hz = swed.ReadFloat(bones + 6 * 32, 8);

                        float w = matrix[12] * hx + matrix[13] * hy + matrix[14] * hz + matrix[15];
                        if (w < 0.01f) continue;
                        float sx = (matrix[0] * hx + matrix[1] * hy + matrix[2] * hz + matrix[3]) / w * halfW + halfW;
                        float sy = -(matrix[4] * hx + matrix[5] * hy + matrix[6] * hz + matrix[7]) / w * halfH + halfH;

                        float dx = sx - centerX, dy = sy - centerY;
                        if (Math.Sqrt(dx * dx + dy * dy) < trigFov)
                        {
                            skaSkjuta = true;
                            break;
                        }
                    }
                }

                if (skaSkjuta)
                {
                    mouse_event(0x0002, 0, 0, 0, 0);
                    Thread.Sleep(50);
                    mouse_event(0x0004, 0, 0, 0, 0);
                    Thread.Sleep(150);
                }
            }
            catch { }

            Thread.Sleep(1);
        }
    });
    triggerbotThread.IsBackground = true;
    triggerbotThread.Start();

    // ── AIM ASSIST [Mouse4 (bak) = håll inne] ─────────────────────
    Thread aimThread = new Thread(() =>
    {
        int debugTick = 0;

        while (true)
        {
            // 0x05 = VK_XBUTTON1 (Mouse4, bakre sidoknapp)
            bool m4Key = (GetAsyncKeyState(0x05) & 0x8000) != 0;
            overlay.aimAktiv = m4Key; // hold, inte toggle

            if (!m4Key) { Thread.Sleep(1); continue; }

            debugTick++;
            if (debugTick % 500 == 0)
                Console.WriteLine($"[AIM DEBUG] lista.Count={lista.Count}");

            try
            {
                float[] matrix = swed.ReadMatrix(client + dwViewMatrix);
                float närmastDist = float.MaxValue;
                float närmastX = 0, närmastY = 0;

                lock (overlay.GetLås())
                {
                    foreach (var p in lista)
                    {
                        if (!p.IsAlive) continue;

                        IntPtr gsn = swed.ReadPointer(p.pawn, m_pGameSceneNode);
                        if (gsn == IntPtr.Zero) continue;
                        IntPtr bones = swed.ReadPointer(gsn + m_modelState, m_boneArray);
                        if (bones == IntPtr.Zero) continue;

                        float hx = swed.ReadFloat(bones + 6 * 32, 0);
                        float hy = swed.ReadFloat(bones + 6 * 32, 4);
                        float hz = swed.ReadFloat(bones + 6 * 32, 8);

                        float w = matrix[12] * hx + matrix[13] * hy + matrix[14] * hz + matrix[15];
                        if (w < 0.01f) continue;
                        float sx = (matrix[0] * hx + matrix[1] * hy + matrix[2] * hz + matrix[3]) / w * halfW + halfW;
                        float sy = -(matrix[4] * hx + matrix[5] * hy + matrix[6] * hz + matrix[7]) / w * halfH + halfH;

                        float dx = sx - centerX, dy = sy - centerY;
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (dist < närmastDist && dist < aimFov)
                        {
                            närmastDist = dist;
                            närmastX = dx;
                            närmastY = dy;
                        }
                    }
                }

                if (närmastDist < aimFov)
                {
                    float moveX = Math.Clamp(närmastX * 2f, -35f, 35f);
                    float moveY = Math.Clamp(närmastY * 2f, -35f, 35f);
                    mouse_event(0x0001, (int)moveX, (int)moveY, 0, 0);
                }
            }
            catch { }

            Thread.Sleep(2);
        }
    });
    aimThread.IsBackground = true;
    aimThread.Start();

    // ── MAIN LOOP ────────────────────────────────────────────────
    while (true)
    {
        Console.SetCursorPosition(0, 0);
        Console.CursorVisible = false;
        Console.ResetColor();
        Console.WriteLine("Namn - Lag - Status    [X = Triggerbot] [Mouse4 håll = Aim]");

        float[] matrix = swed.ReadMatrix(client + dwViewMatrix);

        lock (overlay.GetLås())
        {
            radar.show(swed, client, lista, matrix);
        }

        Thread.Sleep(1);
    }
}

[System.Runtime.InteropServices.DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);

[System.Runtime.InteropServices.DllImport("user32.dll")]
static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);