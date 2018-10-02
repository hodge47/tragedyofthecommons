using UnityEngine;
public class Family : MonoBehaviour{
    public string familyName = string.Empty;
    public int numberOfFamilyMembers = 0;

    public int neededRations = 0;
    public int currentRations = 0;
    public int consumeRations = 0;
    public int storedRations = 0;
    public float rationsPerMember = 0;
    public int rationsStoleOverTime = 0;

    public int totalRationsThisYear = 0;
    public int totalRationsAllTime = 0;

    public int overallFamilyHealth = 100; // Percent

    public bool dead = false;

    public int yearOfDeath = 0;

    public void CalculateRationsPerMember()
    {
        rationsPerMember = (float)consumeRations / (float)numberOfFamilyMembers;
        if(rationsPerMember < 0.7)
        {
            overallFamilyHealth -= 10;
            if(overallFamilyHealth <= 0){
                dead = true;
            }
        }
    }
    public void CalculateNourishment()
    {

    }
}
