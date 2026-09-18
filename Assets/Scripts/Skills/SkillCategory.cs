using System;

[Flags]
public enum SkillCategory
{
    None       = 0,
    Projectile = 1 << 0,
    Area       = 1 << 1,
    Piercing   = 1 << 2,
    Chain      = 1 << 3,
    DoT        = 1 << 4,
    Melee      = 1 << 5
}