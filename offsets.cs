// Uppdaterad 2026-07-10 | a2x/cs2-dumper 2026-07-10 07:51:31 UTC
// Bekräftad stride: 0x70 (via debug)
// Bekräftad m_iszPlayerName: 0x6E0 (via debug)

static class Offsets
{
    // client.dll – globala pekare
    public const int dwEntityList               =  0x2572230;  // UPPDATERAD
    public const int dwViewMatrix               = 0x23CC830;  // UPPDATERAD
    public const int dwLocalPlayerController    = 0x23A1F30;  // UPPDATERAD
    public const int dwLocalPlayerPawn          = 0x23C7268;  // UPPDATERAD

    // CBasePlayerController (bekräftad via debug)
    public const int m_iszPlayerName            = 0x6F4;
    public const int m_bIsLocalPlayerController = 0x788;

    // CCSPlayerController
    public const int m_hPlayerPawn              = 0x914;
    public const int m_bPawnIsAlive             = 0x91C;

    // C_BaseEntity / pawn
    public const int m_iTeamNum                 = 0x3E7;
    public const int m_iHealth                  = 0x34C;
    public const int m_vOldOrigin               = 0x13B8;
    public const int m_pGameSceneNode           = 0x330;
    public const int m_vecAbsOrigin             = 0xC8;      // CGameSceneNode.m_vecAbsOrigin

    // Spotted (C_CSPlayerPawn)
    public const int m_entitySpottedState       = 0x11B0;    // UPPDATERAD
    public const int m_bSpotted                 = 0x8;

    // ESP / skelett
    public const int m_modelState               = 0x140;
    public const int m_boneArray                = 0x80;

    // Vapen (C_BasePlayerWeapon)
    public const int m_pWeaponServices          = 0x1208;
    public const int m_hActiveWeapon            = 0x60;
    public const int m_iClip1                   = 0x1700;    // UPPDATERAD
    public const int m_szName                   = 0x720;
    public const int m_AttributeManager         = 0x1148;
    public const int m_Item                     = 0x50;
    public const int m_iItemDefinitionIndex     = 0x1BA;

    // Anti flash (C_CSPlayerPawnBase)
    public const int m_flFlashMaxAlpha          = 0x1424;    // UPPDATERAD
    public const int m_flFlashDuration          = 0x1428;    // UPPDATERAD
}