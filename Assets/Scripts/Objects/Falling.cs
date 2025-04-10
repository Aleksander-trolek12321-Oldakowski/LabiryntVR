using UnityEngine;

namespace Objects
{
    public class Falling : MonoBehaviour
    {
        public GameObject Interaction;
        public GameObject Beam;

        public Collider Player;

        private void OnTriggerEnter(Collider other) 
        {
            if(other == Player)
            {
                Beam.SetActive(true);
                Interaction.SetActive(false);
            }
        }
    }
}