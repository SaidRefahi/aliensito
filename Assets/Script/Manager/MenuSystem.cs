using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MenuSystem : MonoBehaviour
{
   public void Play()
   {
      SceneManager.LoadScene(3);
   }
   public void Quit()
   {
      Application.Quit();
      UnityEditor.EditorApplication.isPlaying = false; // tengo que sacarlo al buildear
      
   }
   
   public void goToMainMenu()
   {
      Time.timeScale = 1f;
      SceneManager.LoadScene(0);
   }
   
}

