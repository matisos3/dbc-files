using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    // =========================================================
    // AKTUALNA INSTANCJA SKILLA
    // =========================================================

    protected SkillInstance skillInstance;


    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public virtual void Initialize(
        SkillInstance instance)
    {
        if (instance == null)
        {
            return;
        }


        if (instance.data == null)
        {
            return;
        }


        skillInstance =
            instance;
    }


    // =========================================================
    // DOSTĘP DO SKILL INSTANCE
    // =========================================================

    protected SkillInstance GetSkillInstance()
    {
        return skillInstance;
    }


    // =========================================================
    // HIT
    // =========================================================

    public abstract void Hit(
        GameObject target);
}