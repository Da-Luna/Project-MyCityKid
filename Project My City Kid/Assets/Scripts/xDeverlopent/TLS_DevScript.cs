using System.Collections;
using UnityEngine;

namespace TLS_DeveloperScripts
{
    public class TLS_DevScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Enable frame rate limiter. If enabled, the frame rate will be limited to the specified maximum.")]
        CharacterController3D m_CharacterController3D;
        
        [Header("General Screen Settings")]
        [SerializeField, Tooltip("Enable frame rate limiter. If enabled, the frame rate will be limited to the specified maximum.")]
        bool limitFrameRate = false;
        [SerializeField, Tooltip("The maximum frame rate when frame rate limiter is active. Values should be in the range of 1 to 120 FPS.")]
        [Range(1, 120)]
        int maxFrameRate = 90;
        [SerializeField, Tooltip("Enable vertical synchronization (VSync). If enabled, the application will synchronize the frame rate with the monitor's refresh rate.")]
        bool activeVSync = false;
        [SerializeField, Tooltip("Vertical synchronization (VSync) setting. 0 = VSync off, 1 = VSync on (one frame per VSync interval), 2 = Double VSync (two frames per VSync interval).")]
        [Range(0, 2)] int vSyncValue = 1;

        [Header("Screen Setting")]
        [SerializeField]
        Vector2 nativeScreenSize = new(1920, 1080);

        [Header("Gameplay Setting")]
        [SerializeField, Range(0.0f, 1.0f)]
        float timeScale = 1.0f;

        [Header("Show Information On HUD")]
        [SerializeField]
        bool showInfosOnGUD = false;

        float fPSCount;

        void Update()
        {
            Time.timeScale = timeScale;

            if (limitFrameRate)
                Application.targetFrameRate = maxFrameRate;

            if (activeVSync)
                QualitySettings.vSyncCount = vSyncValue;
        }

        IEnumerator Start()
        {
            Debug.LogError("### Grounded Status : Wenn der Spieler gegen eine Wand rennt und Springt, gibt ein Boost. Mehrfache sprünge möglich");
            Debug.LogError("### Grounded Status : Wenn der Spieler gegen eine Wand rennt, die bis zu der Hüfte geht und Springt, wird die Lande-Animation ausgelöst");
            Debug.LogError("### Input Movement > Animation: Normales rennen muss bei 1f sein. Mit Sprint bei 1.2f. Wenn der Input-Wert unter 0.9f fällt, muss Sprinten unterbrochen werden!");
            Debug.LogError("### OnStairs Bool: Wenn man auf einer Treppe steht und springt, wird der Bool nicht auf false gesetzt!");
            Debug.LogError("### Gravity: Spieler-Momentum-Variable?");

            GUI.depth = 2;
            while (true)
            {
                fPSCount = 1f / Time.unscaledDeltaTime;
                yield return new WaitForSeconds(0.1f);
            }
        }

        void OnGUI()
        {
            if (showInfosOnGUD)
            {
                Vector3 scale = new(Screen.width / nativeScreenSize.x, Screen.height / nativeScreenSize.y, 1.0f);
                GUI.matrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, scale);

                GUIStyle myFontSize = new()
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold
                };

                myFontSize.normal.textColor = Color.yellow;

                GUI.Label(new Rect(1700, 100, 100, 20), "Current FPS: " + Mathf.Round(fPSCount), myFontSize);

                GUI.Label(new Rect(100, 100, 100, 20), "Move Vector: " + m_CharacterController3D.MoveVector.ToString(), myFontSize);
                GUI.Label(new Rect(100, 125, 100, 20), "Move Vector Extern: " + m_CharacterController3D.ExternalMoveVector.ToString(), myFontSize);
                GUI.Label(new Rect(100, 150, 100, 20), "Gravity: " + m_CharacterController3D.GravityVelocity.ToString("F1"), myFontSize);
                GUI.Label(new Rect(100, 175, 100, 20), "Grounded: " + PlayerCharacter.PlayerInstance.GroundedCheck().ToString(), myFontSize);
                GUI.Label(new Rect(100, 200, 100, 20), "Sliding: " + m_CharacterController3D.IsSliding.ToString(), myFontSize);
                GUI.Label(new Rect(100, 225, 100, 20), "Ground Angle: " + m_CharacterController3D.GroundAngle.ToString(), myFontSize);
                GUI.Label(new Rect(100, 250, 100, 20), "On Stairs: " + m_CharacterController3D.IsOnStairs.ToString(), myFontSize);
            }
        }
    }
}