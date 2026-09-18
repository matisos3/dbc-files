using System.Collections.Generic;
using UnityEngine;

public class SpinningLaserController : MonoBehaviour
{
    // =========================================================
    // POZYCJA LASERA
    // =========================================================

    [Header("Pozycja lasera")]
    [SerializeField]
    private float laserHeight = 0.5f;


    // =========================================================
    // DANE
    // =========================================================

    private SkillInstance skill;
    private Transform towerTransform;


    // =========================================================
    // DANE POJEDYNCZEGO RAMIENIA
    // =========================================================

    private class LaserData
    {
        public GameObject laser;

        public Transform beam;

        public Vector3 baseScale;

        // Własna liczba pozostałych przebić
        public int remainingPierces;

        // Własna lista przeciwników przebitych przez to ramię
        public HashSet<EnemyHealth> piercedEnemies =
            new HashSet<EnemyHealth>();

        // Własne timery obrażeń tego ramienia
        public Dictionary<EnemyHealth, float> damageTimers =
            new Dictionary<EnemyHealth, float>();

        // Stały kąt początkowy ramienia
        public float initialAngle;

        // =====================================================
        // WŁASNY COOLDOWN ODNOWIENIA
        // =====================================================

        public float respawnTimer;

        public bool waitingForRespawn;
    }


    // =========================================================
    // RAMIONA
    // =========================================================

    private readonly List<LaserData> laserData =
        new List<LaserData>();


    // =========================================================
    // ROTACJA
    // =========================================================

    private float currentAngle = 0f;


    private bool initialized = false;


    // =========================================================
    // OSTATNIA LICZBA RAMION
    // =========================================================

    // Zapamiętujemy liczbę ramion z poprzedniej klatki.
    //
    // Dzięki temu po zakupie upgrade'u:
    //
    // 2 -> 3
    //
    // kontroler wykryje zmianę i utworzy dodatkowe ramię.
    // =========================================================

    private int lastLaserCount = 0;


    // =========================================================
    // INFORMACJA O INICJALIZACJI
    // =========================================================

    public bool IsInitialized
    {
        get
        {
            return initialized;
        }
    }


    // =========================================================
    // INICJALIZACJA
    // =========================================================

    public void Initialize(
        SkillInstance skillInstance,
        Transform newTowerTransform)
    {
        if (initialized)
            return;


        if (skillInstance == null)
        {
            return;
        }


        if (skillInstance.data == null)
        {
            return;
        }


        skill = skillInstance;

        towerTransform = newTowerTransform;


        if (towerTransform == null)
        {
            return;
        }


        if (skill.spinningLaserPrefab == null)
        {
            return;
        }


        // =====================================================
        // ZABEZPIECZENIA
        // =====================================================

        skill.spinningLaserCount =
            Mathf.Max(
                1,
                skill.spinningLaserCount
            );


        skill.spinningLaserRadius =
            Mathf.Max(
                0f,
                skill.spinningLaserRadius
            );


        skill.spinningLaserRotationSpeed =
            Mathf.Max(
                0f,
                skill.spinningLaserRotationSpeed
            );


        skill.spinningLaserDamage =
            Mathf.Max(
                0f,
                skill.spinningLaserDamage
            );


        skill.spinningLaserDamageInterval =
            Mathf.Max(
                0.05f,
                skill.spinningLaserDamageInterval
            );


        skill.spinningLaserWidth =
            Mathf.Max(
                0.001f,
                skill.spinningLaserWidth
            );


        // =====================================================
        // WYCZYŚĆ STARE DANE
        // =====================================================

        laserData.Clear();

        currentAngle = 0f;


        // =====================================================
        // UTWÓRZ WSZYSTKIE RAMIONA
        // =====================================================

        CreateLasers();


        if (laserData.Count == 0)
        {
            initialized = false;

            Destroy(this);

            return;
        }


        // =====================================================
        // ZAPAMIĘTAJ AKTUALNĄ LICZBĘ RAMION
        // =====================================================

        lastLaserCount =
            skill.spinningLaserCount;


        initialized = true;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!initialized)
            return;


        if (skill == null ||
            skill.data == null)
        {
            Destroy(this);
            return;
        }


        if (towerTransform == null)
        {
            Destroy(this);
            return;
        }


        // =====================================================
        // SPRAWDŹ CZY ZMIENIŁA SIĘ LICZBA RAMION
        // =====================================================

        SynchronizeLaserCount();


        // =====================================================
        // ROTACJA
        // =====================================================

        currentAngle +=
            skill.spinningLaserRotationSpeed *
            Time.deltaTime;


        if (currentAngle >= 360f)
        {
            currentAngle -= 360f;
        }


        // =====================================================
        // AKTUALIZACJA RAMION
        // =====================================================

        UpdateLasers();
    }


    // =========================================================
    // SYNCHRONIZACJA LICZBY RAMION
    // =========================================================

    private void SynchronizeLaserCount()
    {
        int desiredCount =
            Mathf.Max(
                1,
                skill.spinningLaserCount
            );


        // Nic się nie zmieniło
        if (desiredCount == lastLaserCount)
            return;


        // =====================================================
        // DODAWANIE RAMION
        // =====================================================

        if (desiredCount > laserData.Count)
        {
            AddLaserArms(
                desiredCount
            );
        }


        // =====================================================
        // USUWANIE RAMION
        // =====================================================

        else if (desiredCount < laserData.Count)
        {
            RemoveLaserArms(
                desiredCount
            );
        }


        // =====================================================
        // ROZSTAW RAMIONA RÓWNOMIERNIE
        // =====================================================

        RecalculateLaserAngles();


        lastLaserCount =
            desiredCount;
    }


    // =========================================================
    // DODAWANIE RAMION
    // =========================================================

    private void AddLaserArms(
        int desiredCount)
    {
        int currentCount =
            laserData.Count;


        if (desiredCount <= currentCount)
            return;


        int piercingCount =
            Mathf.Max(
                1,
                skill.piercingCount
            );


        // =====================================================
        // DODAJ TYLKO BRAKUJĄCE RAMIONA
        // =====================================================

        for (int i = currentCount;
             i < desiredCount;
             i++)
        {
            LaserData data =
                new LaserData();


            data.initialAngle =
                0f;


            data.remainingPierces =
                piercingCount;


            data.respawnTimer =
                0f;


            data.waitingForRespawn =
                false;


            // =================================================
            // NAJPIERW DODAJ DO LISTY
            // =================================================

            laserData.Add(
                data
            );


            // =================================================
            // UTWÓRZ PREFAB
            // =================================================

            bool spawned =
                SpawnLaserArm(
                    data
                );


            if (!spawned)
            {
                data.laser = null;
                data.beam = null;

                data.remainingPierces = 0;

                data.respawnTimer = 0f;

                data.waitingForRespawn = true;
            }
        }
    }


    // =========================================================
    // USUWANIE RAMION
    // =========================================================

    private void RemoveLaserArms(
        int desiredCount)
    {
        while (laserData.Count > desiredCount)
        {
            int index =
                laserData.Count - 1;


            LaserData data =
                laserData[index];


            if (data != null)
            {
                if (data.laser != null)
                {
                    Destroy(
                        data.laser
                    );
                }


                if (data.piercedEnemies != null)
                {
                    data.piercedEnemies.Clear();
                }


                if (data.damageTimers != null)
                {
                    data.damageTimers.Clear();
                }
            }


            laserData.RemoveAt(
                index
            );
        }
    }


    // =========================================================
    // PRZELICZENIE KĄTÓW RAMION
    // =========================================================

    private void RecalculateLaserAngles()
    {
        int count =
            laserData.Count;


        if (count <= 0)
            return;


        float angleStep =
            360f / count;


        for (int i = 0;
             i < count;
             i++)
        {
            LaserData data =
                laserData[i];


            if (data == null)
                continue;


            data.initialAngle =
                angleStep * i;
        }
    }


    // =========================================================
    // TWORZENIE WSZYSTKICH RAMION
    // =========================================================

    private void CreateLasers()
    {
        int laserCount =
            Mathf.Max(
                1,
                skill.spinningLaserCount
            );

        float angleStep =
            360f / laserCount;


        int piercingCount =
            Mathf.Max(
                1,
                skill.piercingCount
            );

        for (int i = 0;
             i < laserCount;
             i++)
        {
            LaserData data =
                new LaserData();


            data.initialAngle =
                angleStep * i;


            data.remainingPierces =
                piercingCount;


            data.respawnTimer =
                0f;


            data.waitingForRespawn =
                false;


            // =================================================
            // DODAJEMY DANE DO LISTY OD RAZU
            // =================================================

            laserData.Add(
                data
            );


            // =================================================
            // PIERWSZE UTWORZENIE
            // =================================================

            bool spawned =
                SpawnLaserArm(
                    data
                );


            if (!spawned)
            {
                data.laser = null;
                data.beam = null;

                data.remainingPierces = 0;

                data.respawnTimer = 0f;

                data.waitingForRespawn = true;
            }
        }


        currentAngle = 0f;
    }


    // =========================================================
    // UTWORZENIE / ODNOWIENIE JEDNEGO RAMIENIA
    // =========================================================

    private bool SpawnLaserArm(
        LaserData data)
    {
        if (data == null)
            return false;


        if (skill == null)
            return false;


        if (towerTransform == null)
            return false;


        if (skill.spinningLaserPrefab == null)
            return false;


        // =====================================================
        // JEŚLI STARY LASER ISTNIEJE
        // =====================================================

        if (data.laser != null)
        {
            Destroy(
                data.laser
            );

            data.laser = null;
        }


        data.beam = null;


        // =====================================================
        // RESET STANU RAMIENIA
        // =====================================================

        data.remainingPierces =
            Mathf.Max(
                1,
                skill.piercingCount
            );


        data.piercedEnemies.Clear();

        data.damageTimers.Clear();


        // =====================================================
        // KĄT RAMIENIA
        // =====================================================

        float angle =
            currentAngle +
            data.initialAngle;


        Vector3 direction =
            Quaternion.Euler(
                0f,
                angle,
                0f
            ) *
            Vector3.forward;


        direction.y = 0f;


        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction =
                Vector3.forward;
        }


        direction.Normalize();


        // =====================================================
        // POCZĄTEK RAMIENIA
        // =====================================================

        Vector3 spawnPosition =
            towerTransform.position +
            direction *
            skill.spinningLaserRadius;


        spawnPosition.y =
            towerTransform.position.y +
            laserHeight;


        // =====================================================
        // UTWÓRZ PREFAB
        // =====================================================

        GameObject laser =
            Instantiate(
                skill.spinningLaserPrefab,
                spawnPosition,
                Quaternion.LookRotation(direction),
                transform
            );


        if (laser == null)
            return false;


        // =====================================================
        // ZNAJDŹ WIĄZKĘ
        // =====================================================

        Transform beam =
            FindBeamTransform(
                laser
            );


        if (beam == null)
        {
            Destroy(
                laser
            );


            return false;
        }


        // =====================================================
        // ZAPISZ DANE
        // =====================================================

        data.laser =
            laser;


        data.beam =
            beam;


        data.baseScale =
            beam.localScale;


        data.waitingForRespawn =
            false;


        data.respawnTimer =
            0f;


        // =====================================================
        // SKALA
        // =====================================================

        ApplyLaserScale(
            data
        );


        // =====================================================
        // DŁUGOŚĆ
        // =====================================================

        float laserLength =
            GetLaserLength(
                laser
            );


        // =====================================================
        // PIVOT W ŚRODKU
        // =====================================================

        Vector3 visualPosition =
            spawnPosition +
            direction *
            (laserLength * 0.5f);


        laser.transform.position =
            visualPosition;


        laser.transform.rotation =
            Quaternion.LookRotation(
                direction
            );

        return true;
    }


    // =========================================================
    // ZNAJDOWANIE WIĄZKI
    // =========================================================

    private Transform FindBeamTransform(
        GameObject laser)
    {
        if (laser == null)
            return null;


        Transform[] children =
            laser.GetComponentsInChildren<Transform>(
                true
            );


        // =====================================================
        // NAJPIERW LASERCORE
        // =====================================================

        foreach (Transform child in children)
        {
            if (child == null)
                continue;


            if (child == laser.transform)
                continue;


            if (child.name == "LaserCore")
            {
                return child;
            }
        }


        // =====================================================
        // AWARYJNIE PIERWSZE DZIECKO
        // =====================================================

        foreach (Transform child in children)
        {
            if (child == null)
                continue;


            if (child == laser.transform)
                continue;


            return child;
        }


        return null;
    }


    // =========================================================
    // AKTUALIZACJA LASERÓW
    // =========================================================

    private void UpdateLasers()
    {
        if (laserData.Count == 0)
            return;


        for (int i = 0;
             i < laserData.Count;
             i++)
        {
            LaserData data =
                laserData[i];


            if (data == null)
                continue;


            // =================================================
            // RAMIĘ CZEKA NA ODNOWIENIE
            // =================================================

            if (data.waitingForRespawn)
            {
                data.respawnTimer -=
                    Time.deltaTime;


                if (data.respawnTimer <= 0f)
                {
                    data.respawnTimer = 0f;


                    SpawnLaserArm(
                        data
                    );
                }


                continue;
            }


            // =================================================
            // AWARYJNIE - PREFAB ZNIKNĄŁ
            // =================================================

            if (data.laser == null)
            {
                StartLaserRespawn(
                    data
                );

                continue;
            }


            // =================================================
            // BRAK PRZEBIĆ
            // =================================================

            if (data.remainingPierces <= 0)
            {
                StartLaserRespawn(
                    data
                );

                continue;
            }


            GameObject laser =
                data.laser;


            // =================================================
            // KĄT
            // =================================================

            float angle =
                currentAngle +
                data.initialAngle;


            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                Vector3.forward;


            direction.y = 0f;


            if (direction.sqrMagnitude <= 0.0001f)
                continue;


            direction.Normalize();


            // =================================================
            // POCZĄTEK RAMIENIA
            // =================================================

            Vector3 startPosition =
                towerTransform.position +
                direction *
                skill.spinningLaserRadius;


            startPosition.y =
                towerTransform.position.y +
                laserHeight;


            // =================================================
            // SKALA
            // =================================================

            ApplyLaserScale(
                data
            );


            // =================================================
            // DŁUGOŚĆ
            // =================================================

            float laserLength =
                GetLaserLength(
                    laser
                );


            // =================================================
            // POZYCJA
            // =================================================

            Vector3 visualPosition =
                startPosition +
                direction *
                (laserLength * 0.5f);


            laser.transform.position =
                visualPosition;


            laser.transform.rotation =
                Quaternion.LookRotation(
                    direction
                );


            // =================================================
            // OBRAŻENIA
            // =================================================

            DamageEnemiesAlongLaser(
                data,
                startPosition,
                direction
            );
        }
    }


    // =========================================================
    // ROZPOCZĘCIE COOLDOWNU JEDNEGO RAMIENIA
    // =========================================================

    private void StartLaserRespawn(
        LaserData data)
    {
        if (data == null)
            return;


        if (data.waitingForRespawn)
            return;


        // =====================================================
        // ZNISZCZ TYLKO TO RAMIĘ
        // =====================================================

        if (data.laser != null)
        {
            Destroy(
                data.laser
            );
        }


        data.laser = null;

        data.beam = null;


        // =====================================================
        // WYCZYŚĆ TYLKO STAN TEGO RAMIENIA
        // =====================================================

        data.piercedEnemies.Clear();

        data.damageTimers.Clear();


        data.remainingPierces = 0;


        // =====================================================
        // WŁASNY COOLDOWN
        // =====================================================

        data.respawnTimer =
            Mathf.Max(
                0f,
                skill.fireRate
            );


        data.waitingForRespawn =
            true;
    }


    // =========================================================
    // SKALOWANIE WIĄZKI
    // =========================================================

    private void ApplyLaserScale(
        LaserData data)
    {
        if (data == null)
            return;


        if (data.beam == null)
            return;


        float aoeMultiplier =
            GetAoEMultiplier();


        data.beam.localScale =
            data.baseScale *
            aoeMultiplier;
    }


    // =========================================================
    // MNOŻNIK AOE
    // =========================================================

    private float GetAoEMultiplier()
    {
        if (skill == null)
            return 1f;


        if (skill.data == null)
            return 1f;


        float baseAoE =
            skill.data.spinningLaserWidth;


        if (baseAoE <= 0.0001f)
            return 1f;


        float currentAoE =
            skill.spinningLaserWidth;


        if (currentAoE <= 0.0001f)
            return 1f;


        float multiplier =
            currentAoE /
            baseAoE;


        return Mathf.Max(
            0.01f,
            multiplier
        );
    }


    // =========================================================
    // OBRAŻENIA WZDŁUŻ RAMIENIA
    // =========================================================

    private void DamageEnemiesAlongLaser(
        LaserData data,
        Vector3 startPosition,
        Vector3 direction)
    {
        if (data == null)
            return;


        if (data.laser == null)
            return;


        if (data.waitingForRespawn)
            return;


        if (data.remainingPierces <= 0)
            return;


        float laserLength =
            GetLaserLength(
                data.laser
            );


        if (laserLength <= 0f)
            return;


        Vector3 endPosition =
            startPosition +
            direction *
            laserLength;


        float laserWidth =
            GetLaserWidth(
                data.laser
            );


        float halfWidth =
            Mathf.Max(
                0.02f,
                laserWidth * 0.5f
            );


        Collider[] hits =
            Physics.OverlapCapsule(
                startPosition,
                endPosition,
                halfWidth,
                ~0,
                QueryTriggerInteraction.Collide
            );


        if (hits == null ||
            hits.Length == 0)
        {
            return;
        }


        HashSet<EnemyHealth> hitEnemiesThisFrame =
            new HashSet<EnemyHealth>();


        foreach (Collider hit in hits)
        {
            if (hit == null)
                continue;


            EnemyHealth enemy =
                hit.GetComponent<EnemyHealth>();


            if (enemy == null)
            {
                enemy =
                    hit.GetComponentInParent<EnemyHealth>();
            }


            if (enemy == null)
                continue;


            if (enemy.IsDying)
                continue;


            if (hitEnemiesThisFrame.Contains(enemy))
                continue;


            hitEnemiesThisFrame.Add(
                enemy
            );


            // =================================================
            // TEN WRÓG ZOSTAŁ JUŻ PRZEBITY PRZEZ TO RAMIĘ
            // =================================================

            if (data.piercedEnemies.Contains(enemy))
                continue;


            // =================================================
            // OBRAŻENIA
            // =================================================

            if (TryDamageEnemy(
                    data,
                    enemy
                ))
            {
                data.piercedEnemies.Add(
                    enemy
                );


                data.remainingPierces--;

                // =================================================
                // TYLKO TO RAMIĘ KOŃCZY SWÓJ CYKL
                // =================================================

                if (data.remainingPierces <= 0)
                {
                    StartLaserRespawn(
                        data
                    );


                    return;
                }
            }
        }
    }


    // =========================================================
    // DŁUGOŚĆ LASERA
    // =========================================================

    private float GetLaserLength(
        GameObject laser)
    {
        if (laser == null)
            return 0.1f;


        Transform beam =
            FindBeamTransform(
                laser
            );


        if (beam == null)
            return 0.1f;


        float length =
            Mathf.Abs(
                beam.localScale.z
            );


        return Mathf.Max(
            0.1f,
            length
        );
    }


    // =========================================================
    // SZEROKOŚĆ LASERA
    // =========================================================

    private float GetLaserWidth(
        GameObject laser)
    {
        if (laser == null)
            return 0.08f;


        Transform beam =
            FindBeamTransform(
                laser
            );


        if (beam == null)
            return 0.08f;


        float widthX =
            Mathf.Abs(
                beam.localScale.x
            );


        float widthY =
            Mathf.Abs(
                beam.localScale.y
            );


        float width =
            Mathf.Min(
                widthX,
                widthY
            );


        return Mathf.Max(
            0.08f,
            width
        );
    }


    // =========================================================
    // OBRAŻENIA W JEDNEGO WROGA
    // =========================================================

    private bool TryDamageEnemy(
        LaserData data,
        EnemyHealth enemy)
    {
        if (data == null)
            return false;


        if (enemy == null)
            return false;


        if (enemy.IsDying)
            return false;


        float currentTime =
            Time.time;


        if (data.damageTimers.TryGetValue(
                enemy,
                out float lastDamageTime))
        {
            if (currentTime - lastDamageTime <
                skill.spinningLaserDamageInterval)
            {
                return false;
            }
        }


        float damage =
            skill.spinningLaserDamage;


        if (damage <= 0f)
            return false;


        data.damageTimers[enemy] =
            currentTime;


        enemy.TakeDamage(
            damage
        );


        return true;
    }


    // =========================================================
    // USUWANIE WSZYSTKICH LASERÓW
    // =========================================================

    private void DestroyExistingLasers()
    {
        for (int i = 0;
             i < laserData.Count;
             i++)
        {
            LaserData data =
                laserData[i];


            if (data == null)
                continue;


            if (data.laser != null)
            {
                Destroy(
                    data.laser
                );
            }


            data.laser = null;

            data.beam = null;


            if (data.piercedEnemies != null)
            {
                data.piercedEnemies.Clear();
            }


            if (data.damageTimers != null)
            {
                data.damageTimers.Clear();
            }


            data.respawnTimer = 0f;

            data.waitingForRespawn = false;

            data.remainingPierces = 0;
        }


        laserData.Clear();
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        DestroyExistingLasers();
    }
}