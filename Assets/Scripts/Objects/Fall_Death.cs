using UnityEngine;

namespace Objects
{
public class Fall_Death : MonoBehaviour
{
    public Collider Player;
    public GameObject playerGameObject;

    void OnTriggerEnter(Collider other)
    {
        if(other == Player)
        {
            Destroy(playerGameObject);
        }
    }
}
}
