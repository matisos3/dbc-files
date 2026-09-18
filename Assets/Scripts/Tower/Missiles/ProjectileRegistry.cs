using System.Collections.Generic;
using UnityEngine;

public static class ProjectileRegistry
{
    private static readonly HashSet<ProjectileMovement> projectiles =
        new HashSet<ProjectileMovement>();


    public static void Register(ProjectileMovement projectile)
    {
        if (projectile == null)
            return;

        projectiles.Add(projectile);
    }


    public static void Unregister(ProjectileMovement projectile)
    {
        if (projectile == null)
            return;

        projectiles.Remove(projectile);
    }


    public static IReadOnlyCollection<ProjectileMovement> Projectiles
    {
        get { return projectiles; }
    }
}