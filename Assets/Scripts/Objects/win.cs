using UnityEngine;
using UnityEngine.SceneManagement;

public class win : MonoBehaviour
{
    [Tooltip("Nazwa sceny menu")]
    public string menuSceneName = "MenuScene"; // <- wpisz tu nazwę swojej sceny menu

    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        if (other.CompareTag("Player"))
        {
            hasWon = true;
            Debug.Log("Gratulacje! Wygrałeś!");
            LoadMenu();
        }
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
