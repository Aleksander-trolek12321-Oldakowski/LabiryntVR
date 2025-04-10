using UnityEngine;

namespace Objects
{
public class Beam : MonoBehaviour
{
    public GameObject g_Player;
    public Collider Player;
    public GameObject Beams;

    void OnTriggerEnter(Collider other)
    {
        if(other == Player)
        {
            Destroy(g_Player);
        }
        else
        {
            Destroy(Beams);
        }
    }
}
}