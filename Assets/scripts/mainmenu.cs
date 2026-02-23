using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using SceneManager = UnityEngine.SceneManagement.SceneManager;
public class mainmenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }
        public void returnMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
      
    }   
}
