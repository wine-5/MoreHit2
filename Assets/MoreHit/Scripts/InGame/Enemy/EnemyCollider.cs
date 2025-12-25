using UnityEngine;

namespace MoreHit
{
    public class EnemyCollider : MonoBehaviour
    {
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            
            
            if (collision.gameObject.CompareTag("Player"))
            {
               
            }
        }

        private void Attack()
        {
            //Ç±Ç±Ç…çUåÇèàóù
        }


    }
}
