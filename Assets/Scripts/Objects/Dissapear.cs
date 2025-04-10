using UnityEngine;

namespace Objects
{
public class Dissapear : MonoBehaviour
{
    public Collider Player;
    public GameObject gameObject;

    void OnTriggerEnter(Collider other)
    {
        if(other == Player)
        {
            gameObject.SetActive(false);
        }
    }
}
}
