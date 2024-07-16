using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [field: SerializeField] public GameObject[] PowerUpPrefabs
    { get; private set; }
    [field: SerializeField] public GameObject[] EnemyPrefabs
    { get; private set; }
    [field: SerializeField] public GameObject EnemyBoss
    { get; private set; }
    [field: SerializeField] public int[] EnemySpawnChances
    { get; private set; }
    public List<GameObject> EnemiesInScene
    { get; private set; } = new List<GameObject>();
    private float SpawnRange
    { get; set; } = 9.0f;
    public int NumberOfEnemiesInWave
    { get; set; }
    public int WaveNumber
    { get; set; }

    private UserInterfaceManager UserInterfaceManagerScript
    { get; set; }
    private PlayerController PlayerControllerScript
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        UserInterfaceManagerScript = GameObject.Find("UserInterfaceManager").GetComponent<UserInterfaceManager>();

        PlayerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!PlayerControllerScript.GameOver)
        {
            SetNewWaveOnClear();
        }
    }

    // Upon spawning of a new enemy, it is added to the List of Enemies active in scene. This allows the amount of enemies in scene to be tracked for sake of re-spawning next wave.
    private void SpawnEnemies(int numberOfEnemies, bool isBossWave)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject enemyPrefabToSpawn;

            if (isBossWave && i == numberOfEnemies - 1)
            {
                enemyPrefabToSpawn = EnemyBoss;
            }
            else
            {
                enemyPrefabToSpawn = EnemyTypeToSpawnChance(EnemyPrefabs, EnemySpawnChances);
            }
            
            GameObject spawnedEnemy = Instantiate(enemyPrefabToSpawn, GenerateSpawnPosition(onBoundryOnly: true), enemyPrefabToSpawn.transform.rotation);
            EnemiesInScene.Add(spawnedEnemy);
        }
    }

    // Once the scene is clear on enemies, increase the number of enemies to spawn next, then spawn the power up and enemies.
    private void SetNewWaveOnClear()
    {
        bool isBossWave = false;

        if (EnemiesInScene.Count == 0)
        {
            // Every new wave should increase the enemy count and the Wave Number.
            NumberOfEnemiesInWave++;

            WaveNumber++;

            // Choose and spawn powerup for the new wave.
            SpawnRandomPrefab();

            // Decide is a boss object should spawn.
            isBossWave = IsBossLevel();

            UserInterfaceManagerScript.UpdateWaveNumberDisplay(WaveNumber, isBossWave);

            SpawnEnemies(NumberOfEnemiesInWave, isBossWave);
        }
    }

    public bool IsBossLevel()
    {
        // Represent how often the Enemy boss should spawn in a Wave.
        // Ever X amount of waves, make the wave spawn a Boss Enemy.
        int bossWaveEvery = 4;

        return (WaveNumber % bossWaveEvery == 0);
    }

    public Vector3 GenerateSpawnPosition(bool onBoundryOnly = false)
    {
        // Assign the X and Z floats to within the SpawnRange.
        float randomSpawnX = Random.Range(-SpawnRange, SpawnRange);
        float randomSpawnZ = Random.Range(-SpawnRange, SpawnRange);

        if (onBoundryOnly == true)
        {
            // 50/50 chance to assign either the X or Y co-ord to either Max or Min float to ensure Spawn Positions remains at edges of spawn boundry.
            if (Random.Range(0, 2) == 0)
            {
                randomSpawnX = Random.Range(0, 2) == 0 ? -SpawnRange : SpawnRange;
            }
            else
            {
                randomSpawnZ = Random.Range(0, 2) == 0 ? -SpawnRange : SpawnRange;
            }
        }


        Vector3 randomSpawnPosition = new Vector3(randomSpawnX, 0, randomSpawnZ);

        return randomSpawnPosition;
    }

    // Method ensures that regardless of the total of the chancesOfSpawningRespectiveEnemies array equalling 100 (as 100 percent), it will always change each figure to become a proportional equal to those totalling of 100.
    // It also accepts any amount of EnemyGameObjects array and SpawnChances array, as long as both arrays need to be the same length.
    private GameObject EnemyTypeToSpawnChance(GameObject[] enemiesSelection, int[] chancesOfSpawningRespectiveEnemies)
    {
        // FIRST SECTION.
        // First Section converts the chancesOfSpawningRespectiveEnemies array figures into figures proportional to those totalling 100.

        int totalPercentage = 0;
        float[] modifiedChanceOfSpawning = new float[chancesOfSpawningRespectiveEnemies.Length];

        // Grab total of all the percentage chances of respective gameObjects to spawn.
        foreach (int percentage in chancesOfSpawningRespectiveEnemies)
        {
            totalPercentage += percentage;
        }

        // Ensures that the chances of spawning for each Enemy GameObject are changed to be propotional to a percentage of 100%.
        for (int i = 0; i < chancesOfSpawningRespectiveEnemies.Length; i++)
        {
            modifiedChanceOfSpawning[i] = SetChancesRelativeTo100Percent(chancesOfSpawningRespectiveEnemies[i], totalPercentage);
        }

        // After changing all the spawn chances to new array, change the total percentage to match 100, which new array total should equal to.
        totalPercentage = (totalPercentage / totalPercentage) * 100;


        // SECOND SECTION.
        // Second section chooses which gameObject to spawn based upon a random percentage number.

        // Below Object will be chosen based on given percentage chances of each object has of spawning.
        GameObject enemyToSpawn = null;
        // Produce a random number from a percentage (0 - 99) range to help select which enemy to spawn.
        int currentSpawnChance = Random.Range(0, totalPercentage);
        // The spawnPercentageCutOff adds up every loop to keep checking if number is below an ever-increasing percentage check.
        float spawnPercentageCutOff = 0.0f;

        // Example:
        // We have four objects with a respective spawn chance of 60%, 20%, 15% and 5%. 
        // In each loop, check if currentSpawnChance is less then the current objects spawn chance number, which in this first loops case is 60.
        // If not, check next objects spawn chance, which being 20, makes the next check being whether currentSpawnChance is below 80 (60 + 20).
        // Then again, if currentSpawnChance is not below 80 either, then check the next objects spawn chance, which being 15 makes the next check at 95 (60 + 20 + 15).
        // Lastly, if currentSpawnChance is not below 95, and only one more object remains, it will check if currentSpawnChance is below 100 (60 + 20 + 15 + 5).
        // Whenever currentSpawnChance is below the spawnPercentageCutOff, that's when the Enemy GameObject relative to the modifiedChanceOfSpawning is assigned to enemyToSpawn.

        for (int i = 0; i < modifiedChanceOfSpawning.Length; i++)
        {
            spawnPercentageCutOff += modifiedChanceOfSpawning[i];

            if (currentSpawnChance < spawnPercentageCutOff)
            {
                enemyToSpawn = enemiesSelection[i];
                break;
            }
        }

        return enemyToSpawn;

        float SetChancesRelativeTo100Percent(int chance, int total)
        {
            float newChanceRelativeTo100 = ((float)chance / total) * 100;
            return newChanceRelativeTo100;
        }
    }

    public void SpawnRandomPrefab()
    {
        int powerUpIndex = Random.Range(0, PowerUpPrefabs.Length);
        Instantiate(PowerUpPrefabs[powerUpIndex], GenerateSpawnPosition(), PowerUpPrefabs[powerUpIndex].transform.rotation);
    }
}
