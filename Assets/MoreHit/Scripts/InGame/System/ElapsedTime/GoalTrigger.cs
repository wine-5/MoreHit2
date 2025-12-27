using UnityEngine;
using MoreHit.ElapsedTime;
using MoreHit.Scene;

/// <summary>
/// �v���C���[�̃S�[�����B�����m���A���Ԍv���̒�~����уV�[���J�ڂ̎��s�𐧌䂷��N���X
/// </summary>

    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField]
        private SceneName nextScene = SceneName.Clear;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // �v����~
                if (ElapsedTimeManager.Instance != null)
                {
                    ElapsedTimeManager.Instance.StopTimer();
                }

                if (SceneController.I != null)
                {
                    // nextSceneに応じて適切なメソッドを呼び出す
                    switch (nextScene)
                    {
                        case SceneName.Clear:
                            SceneController.I.ChangeToGameClearScene();
                            break;
                        case SceneName.GameOver:
                            SceneController.I.ChangeToGameOverScene();
                            break;
                        case SceneName.Title:
                            SceneController.I.ChangeToTitleScene();
                            break;
                        case SceneName.InGame:
                            SceneController.I.ChangeToInGameScene();
                            break;
                        default:
                            SceneController.I.LoadScene(nextScene);
                            break;
                    }
                }
                else
                {

                    Debug.LogError("SceneController�����݂��܂���B");
                }
            }
        }
    }
