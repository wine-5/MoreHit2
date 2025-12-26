using UnityEngine;
using UnityEngine.SceneManagement;
using MoreHit;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private string resultSceneName = "Poro256-test";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Œv‘ª’â~
            if (ElapsedTime.Instance != null)
            {
                ElapsedTime.Instance.StopTimer();
            }

            // ƒV[ƒ“‘JˆÚ
            SceneManager.LoadScene(resultSceneName);
        }
    }
}