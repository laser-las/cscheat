using Swed64;
using static Offsets;

public class Radar
{
    public void show(Swed swed, IntPtr client, List<Info> lista, float[] matrix)
    {
        lista.Clear();
        IntPtr entityList = swed.ReadPointer(client, dwEntityList);
        IntPtr listEntry = swed.ReadPointer(entityList, 0x10);
        int myTeam = 0;

        for (int i = 0; i < 64; i++)
        {
            if (listEntry == IntPtr.Zero) continue;
            IntPtr controller = swed.ReadPointer(listEntry, i * 0x70);
            if (controller == IntPtr.Zero) continue;
            if (!swed.ReadBool(controller, m_bIsLocalPlayerController)) continue;

            myTeam = swed.ReadInt(controller, m_iTeamNum);
            break;
        }

        if (myTeam == 0) return;

        for (int i = 0; i < 64; i++)
        {
            if (listEntry == IntPtr.Zero) continue;
            IntPtr currentController = swed.ReadPointer(listEntry, i * 0x70);
            if (currentController == IntPtr.Zero) continue;
            int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
            if (pawnHandle == 0) continue;
            IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
            IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x70 * (pawnHandle & 0x1FF));
            if (currentPawn == IntPtr.Zero) continue;

            bool islocalplayer = swed.ReadBool(currentController, m_bIsLocalPlayerController);
            if (islocalplayer) continue;

            int team = swed.ReadInt(currentController, m_iTeamNum);
            if (team == myTeam) continue;

            bool isAlive = swed.ReadBool(currentController, m_bPawnIsAlive);
            int hp = swed.ReadInt(currentPawn, m_iHealth);
            string name = swed.ReadString(currentController, m_iszPlayerName, 16);

            bool spotted = false;
            try { spotted = swed.ReadBool(currentPawn, m_entitySpottedState + m_bSpotted); } catch { }

            int ammo = 0;
            string vapen = "";
            try
            {
                IntPtr weapSvc = swed.ReadPointer(currentPawn, m_pWeaponServices);
                if (weapSvc != IntPtr.Zero)
                {
                    int weaponHandle = swed.ReadInt(weapSvc, m_hActiveWeapon);
                    IntPtr weaponEntry = swed.ReadPointer(entityList, 0x8 * ((weaponHandle & 0x7FFF) >> 9) + 0x10);
                    IntPtr weaponPawn = swed.ReadPointer(weaponEntry, 0x70 * (weaponHandle & 0x1FF));
                    if (weaponPawn != IntPtr.Zero)
                    {
                        ammo = swed.ReadInt(weaponPawn, m_iClip1);
                        int defIndex = swed.ReadInt(weaponPawn, m_AttributeManager + m_Item + m_iItemDefinitionIndex);
                        vapen = HämtaVapenNamn(defIndex);
                    }
                }
            }
            catch { }

            var pos = swed.ReadVec(currentPawn, m_vOldOrigin);

            Info p = new Info();
            p.Name = name;
            p.Team = team;
            p.IsAlive = isAlive;
            p.IsSpotted = spotted;
            p.IsLocalPlayer = false;
            p.posX = pos.X;
            p.posY = pos.Y;
            p.posZ = pos.Z;
            p.matrix = matrix;
            p.hp = hp;
            p.pawn = currentPawn;
            p.vapen = vapen;
            p.ammo = ammo;
            lista.Add(p);

        }
    }

    string HämtaVapenNamn(int defIndex)
    {
        switch (defIndex)
        {
            case 1:  return "Desert Eagle";
            case 2:  return "Dual Berettas";
            case 3:  return "Five-SeveN";
            case 4:  return "Glock";
            case 7:  return "AK-47";
            case 8:  return "AUG";
            case 9:  return "AWP";
            case 10: return "FAMAS";
            case 11: return "G3SG1";
            case 13: return "Galil AR";
            case 14: return "M249";
            case 16: return "M4A4";
            case 17: return "MAC-10";
            case 19: return "P90";
            case 23: return "MP5";
            case 24: return "UMP-45";
            case 25: return "XM1014";
            case 26: return "PP-Bizon";
            case 27: return "MAG-7";
            case 28: return "Negev";
            case 29: return "Sawed-Off";
            case 30: return "Tec-9";
            case 31: return "Zeus";
            case 32: return "P2000";
            case 33: return "MP7";
            case 34: return "MP9";
            case 35: return "Nova";
            case 36: return "P250";
            case 38: return "SCAR-20";
            case 39: return "SG 553";
            case 40: return "SSG 08";
            case 60: return "M4A1-S";
            case 61: return "USP-S";
            case 63: return "CZ75";
            case 64: return "R8";
            default: return "Okänt";
        }
    }
}