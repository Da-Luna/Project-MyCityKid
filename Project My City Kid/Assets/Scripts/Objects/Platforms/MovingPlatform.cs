using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform reachPosition;
    public float oneWayInSeconds = 5;

    protected Vector3 gameObjFrom;
    protected Vector3 gameObjTo;

    protected bool playerOnPlatform;

    protected CharacterController3D m_CharacterController3D;
    protected Rigidbody m_Rigidbody;

    protected Vector3 previousPosition; // Letzte Position der Plattform

    void Start()
    {
        Debug.LogError("NullReferenceException: OnTriggerExit() : m_CharacterController3D.ExternalMove(Vector3.zero)");
        m_Rigidbody = GetComponent<Rigidbody>();

        gameObjFrom = transform.position;
        gameObjTo = reachPosition.position;
        previousPosition = transform.position; // Initiale Position speichern
    }

    void FixedUpdate()
    {
        // Bewegung der Plattform
        transform.position = Vector3.Lerp(gameObjFrom, gameObjTo, Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / oneWayInSeconds, 1f)));

        // Wende die Bewegung auf den Spieler an, falls er auf der Plattform steht
        if (playerOnPlatform && m_CharacterController3D != null)
        {
            Vector3 platformMovement = (transform.position - previousPosition) / Time.fixedDeltaTime; // Berechne den Unterschied zur letzten Position
            previousPosition = transform.position; // Speichere die aktuelle Position für den nächsten Frame

            m_CharacterController3D.ExternalMove(platformMovement);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Prüfen, ob es der Spieler ist
        if (other.CompareTag("Player"))
        {
            m_CharacterController3D = other.GetComponent<CharacterController3D>();
            previousPosition = transform.position;

            playerOnPlatform = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Prüfen, ob es der Spieler ist
        if (other.CompareTag("Player"))
        {
            m_CharacterController3D.ExternalMove(Vector3.zero);
            previousPosition = Vector3.zero;

            m_CharacterController3D = null;
            playerOnPlatform = false;
        }
    }
}
