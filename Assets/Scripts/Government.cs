using UnityEngine;
public class Government : MonoBehaviour{

    public int availableRations = 100;
    public int allowedRationsPerFamily = 4;
    public int stolenRations = 0;

    public int numberOfFamilies = 25;
    public int numberOfStarvingFamilies = 0;
    public int numberOfDeadFamilies = 0;
}
